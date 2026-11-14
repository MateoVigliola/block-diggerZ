/**
 * BlockDiggerz Asset Tracker: Google Sheets side.
 *
 * Paste this whole file into the sheet's Extensions > Apps Script editor, save, reload the sheet,
 * then use the "Asset Tracker" menu. Full steps: Assets/_Project/AssetTracking/Sheet/SETUP.md.
 *
 * Column ownership:
 *   Unity writes: ID, Area, Asset Path, Asset Type, Updated In Unity (grey headers, warning on edit).
 *   People write: Name, Description, Requirements, Status, Notes.
 *   Extra columns you add are kept and never touched.
 *
 * Planned assets: add a row with at least a Name. The next Unity sync gives it an ID.
 * Keep the protocol names (pull, push, JSON field keys, status names) in step with SheetMessages.cs,
 * SheetSyncMerge.cs and AssetStatusNames.cs.
 */

const TABS = ['Art', 'Audio'];
const ID_HEADER = 'ID';
const NAME_HEADER = 'Name';
const AREA_HEADER = 'Area';
const STATUS_HEADER = 'Status';
const PATH_HEADER = 'Asset Path';
const TYPE_HEADER = 'Asset Type';
const UPDATED_HEADER = 'Updated In Unity';
const HEADERS = [ID_HEADER, NAME_HEADER, AREA_HEADER, 'Description', 'Requirements', STATUS_HEADER, 'Notes',
  PATH_HEADER, TYPE_HEADER, UPDATED_HEADER];
const UNITY_HEADERS = [ID_HEADER, AREA_HEADER, PATH_HEADER, TYPE_HEADER, UPDATED_HEADER];
const TEXT_FORMAT_HEADERS = [ID_HEADER, PATH_HEADER, UPDATED_HEADER];
const FIELD_HEADERS = {
  name: NAME_HEADER,
  description: 'Description',
  requirements: 'Requirements',
  status: STATUS_HEADER,
  notes: 'Notes'
};
const STATUSES = ['Missing', 'Placeholder', 'Needs Replacement', 'Final'];
const STATUS_COLORS = {
  'Missing': '#f4c7c3',
  'Placeholder': '#fce8b2',
  'Needs Replacement': '#f9cb9c',
  'Final': '#b7e1cd'
};
const UNITY_HEADER_COLOR = '#d9d9d9';
const PEOPLE_HEADER_COLOR = '#cfe2f3';
const PROTECTION_DESCRIPTION = 'Asset Tracker: written by Unity';
const SECRET_PROPERTY = 'SYNC_SECRET';
const SHEET_ID_PREFIX = 'S-';
const LOCK_TIMEOUT_MS = 30000;
const ACTION_PULL = 'pull';
const ACTION_PUSH = 'push';

function onOpen() {
  SpreadsheetApp.getUi()
    .createMenu('Asset Tracker')
    .addItem('Set up tabs', 'setupTabs')
    .addItem('Generate sync secret', 'generateSecret')
    .addToUi();
}

/** Creates or repairs the Art and Audio tabs. Safe to run again; existing rows and extra columns are kept. */
function setupTabs() {
  const spreadsheet = SpreadsheetApp.getActive();
  TABS.forEach(function (name) {
    setupTab(spreadsheet, name);
  });
  SpreadsheetApp.getUi().alert('The Art and Audio tabs are ready.');
}

/** Creates a new sync secret. Paste it into Unity: Asset Tracker window > Settings > Sync Secret. */
function generateSecret() {
  const secret = (Utilities.getUuid() + Utilities.getUuid()).replace(/-/g, '');
  PropertiesService.getScriptProperties().setProperty(SECRET_PROPERTY, secret);
  SpreadsheetApp.getUi().alert(
    'New sync secret (copy it now, it is not shown again):\n\n' + secret +
    '\n\nPaste it into Unity: Tools > BlockDiggerz > Asset Tracker > Settings. ' +
    'Anyone with an older secret can no longer sync.');
}

/** Lets you check the deployment URL in a browser. */
function doGet() {
  return ContentService.createTextOutput('BlockDiggerz Asset Tracker web app is running.');
}

function doPost(e) {
  const lock = LockService.getDocumentLock();
  try {
    lock.waitLock(LOCK_TIMEOUT_MS);
    const request = JSON.parse(e.postData.contents);
    checkSecret(request.secret);
    if (request.action === ACTION_PULL) {
      return json(pull());
    }
    if (request.action === ACTION_PUSH) {
      return json(push(request));
    }
    throw new Error('Unknown action: ' + request.action);
  } catch (error) {
    return json({ ok: false, error: String(error && error.message ? error.message : error) });
  } finally {
    lock.releaseLock();
  }
}

function checkSecret(secret) {
  const expected = PropertiesService.getScriptProperties().getProperty(SECRET_PROPERTY);
  if (!expected) {
    throw new Error('No sync secret set. In the sheet, run Asset Tracker > Generate sync secret.');
  }
  if (secret !== expected) {
    throw new Error('Wrong sync secret. Copy the current one into Unity.');
  }
}

/** Returns every named row of both tabs, giving new rows an ID first. */
function pull() {
  const rows = [];
  TABS.forEach(function (tab) {
    const table = readTable(tab);
    let assigned = false;
    table.rows.forEach(function (row) {
      if (isBlankRow(row)) {
        return;
      }
      let id = cell(table, row, ID_HEADER);
      if (!id) {
        if (!cell(table, row, NAME_HEADER)) {
          return;
        }
        id = SHEET_ID_PREFIX + Utilities.getUuid().replace(/-/g, '');
        row[table.map[ID_HEADER]] = id;
        assigned = true;
      }
      rows.push({
        id: id,
        category: tab,
        name: cell(table, row, NAME_HEADER),
        area: cell(table, row, AREA_HEADER),
        description: cell(table, row, FIELD_HEADERS.description),
        requirements: cell(table, row, FIELD_HEADERS.requirements),
        status: cell(table, row, STATUS_HEADER),
        notes: cell(table, row, FIELD_HEADERS.notes)
      });
    });
    if (assigned) {
      writeIdColumn(table);
    }
  });
  return { ok: true, sheetUrl: SpreadsheetApp.getActive().getUrl(), rows: rows };
}

/** Writes Unity's rows, moves rows between tabs when their category changed, and deletes removed IDs. */
function push(request) {
  const tables = {};
  const index = {};
  TABS.forEach(function (tab) {
    const table = readTable(tab);
    tables[tab] = table;
    table.rows.forEach(function (row) {
      const id = cell(table, row, ID_HEADER);
      if (id) {
        index[id] = { tab: tab, row: row };
      }
    });
  });

  let deleted = 0;
  (request.deleteIds || []).forEach(function (id) {
    const hit = index[id];
    if (hit) {
      removeRow(tables[hit.tab], hit.row);
      delete index[id];
      deleted++;
    }
  });

  let written = 0;
  (request.rows || []).forEach(function (pushed) {
    const tab = TABS.indexOf(pushed.category) >= 0 ? pushed.category : TABS[0];
    const table = tables[tab];
    const hit = index[pushed.id];
    let row;
    if (hit && hit.tab === tab) {
      row = hit.row;
    } else {
      row = new Array(table.width).fill('');
      if (hit) {
        const from = tables[hit.tab];
        Object.keys(from.map).forEach(function (header) {
          if (header in table.map) {
            row[table.map[header]] = hit.row[from.map[header]];
          }
        });
        removeRow(from, hit.row);
      }
      table.rows.push(row);
      index[pushed.id] = { tab: tab, row: row };
    }

    setCell(table, row, ID_HEADER, pushed.id);
    setCell(table, row, AREA_HEADER, pushed.area);
    setCell(table, row, PATH_HEADER, pushed.assetPath);
    setCell(table, row, TYPE_HEADER, pushed.assetType);
    setCell(table, row, UPDATED_HEADER, pushed.unityUpdated);
    (pushed.writeFields || []).forEach(function (key) {
      const header = FIELD_HEADERS[key];
      if (header) {
        setCell(table, row, header, pushed[key]);
      }
    });
    written++;
  });

  TABS.forEach(function (tab) {
    writeTable(tables[tab]);
  });
  return { ok: true, sheetUrl: SpreadsheetApp.getActive().getUrl(), written: written, deleted: deleted };
}

function setupTab(spreadsheet, name) {
  const sheet = spreadsheet.getSheetByName(name) || spreadsheet.insertSheet(name);
  const map = headerMap(sheet);
  let nextColumn = sheet.getLastColumn() + 1;
  HEADERS.forEach(function (header) {
    if (!(header in map)) {
      sheet.getRange(1, nextColumn).setValue(header);
      map[header] = nextColumn - 1;
      nextColumn++;
    }
  });

  const maxRows = sheet.getMaxRows();
  const dataRows = Math.max(maxRows - 1, 1);
  sheet.setFrozenRows(1);
  sheet.getRange(1, 1, 1, sheet.getLastColumn()).setFontWeight('bold');

  HEADERS.forEach(function (header) {
    const isUnity = UNITY_HEADERS.indexOf(header) >= 0;
    sheet.getRange(1, map[header] + 1).setBackground(isUnity ? UNITY_HEADER_COLOR : PEOPLE_HEADER_COLOR);
  });

  TEXT_FORMAT_HEADERS.forEach(function (header) {
    sheet.getRange(2, map[header] + 1, dataRows, 1).setNumberFormat('@');
  });

  const statusRange = sheet.getRange(2, map[STATUS_HEADER] + 1, dataRows, 1);
  statusRange.setDataValidation(SpreadsheetApp.newDataValidation()
    .requireValueInList(STATUSES, true)
    .setAllowInvalid(false)
    .build());

  const keptRules = sheet.getConditionalFormatRules().filter(function (rule) {
    const condition = rule.getBooleanCondition();
    if (!condition || condition.getCriteriaType() !== SpreadsheetApp.BooleanCriteria.TEXT_EQUAL_TO) {
      return true;
    }
    return STATUSES.indexOf(String(condition.getCriteriaValues()[0])) < 0;
  });
  STATUSES.forEach(function (status) {
    keptRules.push(SpreadsheetApp.newConditionalFormatRule()
      .whenTextEqualTo(status)
      .setBackground(STATUS_COLORS[status])
      .setRanges([statusRange])
      .build());
  });
  sheet.setConditionalFormatRules(keptRules);

  sheet.getProtections(SpreadsheetApp.ProtectionType.RANGE).forEach(function (protection) {
    if (protection.getDescription() === PROTECTION_DESCRIPTION) {
      protection.remove();
    }
  });
  UNITY_HEADERS.forEach(function (header) {
    sheet.getRange(2, map[header] + 1, dataRows, 1)
      .protect()
      .setDescription(PROTECTION_DESCRIPTION)
      .setWarningOnly(true);
  });
}

function readTable(tab) {
  const sheet = SpreadsheetApp.getActive().getSheetByName(tab);
  if (!sheet) {
    throw new Error('Missing tab "' + tab + '". In the sheet, run Asset Tracker > Set up tabs.');
  }
  const map = headerMap(sheet);
  HEADERS.forEach(function (header) {
    if (!(header in map)) {
      throw new Error('Tab "' + tab + '" has no "' + header + '" column. Run Asset Tracker > Set up tabs.');
    }
  });

  const width = sheet.getLastColumn();
  const lastRow = sheet.getLastRow();
  let rows = [];
  if (lastRow > 1) {
    const range = sheet.getRange(2, 1, lastRow - 1, width);
    const values = range.getValues();
    const formulas = range.getFormulas();
    rows = values.map(function (row, r) {
      return row.map(function (value, c) {
        return formulas[r][c] || value;
      });
    });
  }
  return { sheet: sheet, map: map, width: width, rows: rows, originalCount: rows.length };
}

function writeTable(table) {
  const count = table.rows.length;
  if (count > 0) {
    table.sheet.getRange(2, 1, count, table.width).setValues(table.rows);
  }
  if (table.originalCount > count) {
    table.sheet.getRange(2 + count, 1, table.originalCount - count, table.width).clearContent();
  }
}

function writeIdColumn(table) {
  if (table.rows.length === 0) {
    return;
  }
  const column = table.map[ID_HEADER];
  table.sheet.getRange(2, column + 1, table.rows.length, 1).setValues(table.rows.map(function (row) {
    return [row[column]];
  }));
}

function headerMap(sheet) {
  const map = {};
  const width = sheet.getLastColumn();
  if (width === 0) {
    return map;
  }
  sheet.getRange(1, 1, 1, width).getValues()[0].forEach(function (header, column) {
    const key = String(header).trim();
    if (key && !(key in map)) {
      map[key] = column;
    }
  });
  return map;
}

function removeRow(table, row) {
  const position = table.rows.indexOf(row);
  if (position >= 0) {
    table.rows.splice(position, 1);
  }
}

function cell(table, row, header) {
  const value = row[table.map[header]];
  return value === undefined || value === null ? '' : String(value).trim();
}

function setCell(table, row, header, value) {
  let text = value === undefined || value === null ? '' : String(value);
  if (text.charAt(0) === '=') {
    text = "'" + text;
  }
  row[table.map[header]] = text;
}

function isBlankRow(row) {
  return row.every(function (value) {
    return value === '' || value === null;
  });
}

function json(payload) {
  return ContentService.createTextOutput(JSON.stringify(payload)).setMimeType(ContentService.MimeType.JSON);
}
