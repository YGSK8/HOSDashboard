const form = document.getElementById("form");
const dbError = document.getElementById("dbInputError");
const deviceError = document.getElementById("deviceInputError");
const guid = document.getElementById("Guid");
const databasename = document.getElementById("DatabaseName");
const serialNo = document.getElementById("SerialNo");
const hardwareId = document.getElementById("HardwareId");
const dateError = document.getElementById("dateInputError");
const fromDate = document.getElementById("_FROMPARTITIONDATE");
const toDate = document.getElementById("_TOPARTITIONDATE");
const run = document.getElementById("run");
const table = document.getElementById("table");
const countLogs = document.getElementById("countoflogs");
const deviceInfo = document.getElementById("deviceInfo");
const databaseInfo = document.getElementById("databaseInfo");
const errorMessage = document.getElementById("errormessage");
const refreshables = document.getElementsByClassName("refreshable");
const download = document.getElementById("download");
let currentSerialNo = "";
let currentDatabaseName = "";
serialNo.value = "G97E12PYY0901";
guid.value = "31725dfc-9b55-4e44-9730-acbfec178f6f";
fromDate.value = "2026-04-01";
toDate.value = "2026-04-02";
run.addEventListener("click", async () => {
    Array.from(refreshables).forEach((element) => {
        let htmlelement = element;
        htmlelement.innerHTML = '';
    });
    if (validateParameters() == true) {
        errorMessage.textContent = '';
        table.innerHTML = 'Loading.....';
        dateError.style.display = 'none';
        deviceError.style.display = 'none';
        dbError.style.display = 'none';
        countLogs.innerHTML = 'Number of rows/count of logs:';
        const Parameters = getParameters();
        const response = await fetch("http://localhost:5260/hos/logs", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(Parameters) });
        if (!response.ok) {
            errorMessage.style.display = 'block';
            const errorData = await response.text();
            errorMessage.textContent = `Status: ${response.status}` + ` Message: ${errorData.toString()}`;
            table.innerHTML = 'Unable to load data';
            return;
        }
        const data = await response.json();
        if (data.dutyStatusLogRecords.length == 0) {
            countLogs.innerHTML = 'Number of rows/count of logs: 0';
            table.innerHTML = '';
            return;
        }
        const firstRecord = data.dutyStatusLogRecords?.[0];
        const keys = firstRecord ? Object.keys(firstRecord) : [];
        countLogs.innerHTML = `Number of rows/count of logs: ${data.dutyStatusLogRecords.length}`;
        deviceInfo.innerHTML = `databaseName: ${data.deviceInfo[0]?.databaseName} serialNo: ${data.deviceInfo[0]?.serialNo} hardwareId: ${data.deviceInfo[0]?.hardwareId} deviceId: ${data.deviceInfo[0]?.deviceId}`;
        currentSerialNo = `${data.deviceInfo[0]?.serialNo}`;
        databaseInfo.innerHTML = `guid: ${data.databaseInfo[0]?.guid} databaseName: ${data.databaseInfo[0]?.databaseName}`;
        currentDatabaseName = `${data.databaseInfo[0]?.databaseName}`;
        let tablerowsHtml = '';
        let firstrowcontent = `<th></th>`;
        keys.forEach(key => {
            firstrowcontent += `<th>${key}</th>`;
        });
        const tableHead = `<thead><tr>${firstrowcontent}</tr></thead>`;
        let count = 1;
        data.dutyStatusLogRecords.forEach(apirecord => {
            const record = mapToDutyStatusLogRecord(apirecord);
            console.log(record);
            tablerowsHtml += `
            <tr>
            <td>${count++}</td>
            <td>${record.Guid}</td>
            <td>${record.ParentId}</td>
            <td>${record.Id}</td>
            <td>${record.DateTime}</td>
            <td>${record.EditDateTime}</td>
            <td>${record.StreamDateTime}</td>
            <td>${record.VerifyDateTime}</td>
            <td>${record.Version}</td>
            <td>${record.Sequence}</td>
            <td>${record.Status}</td>
            <td>${record.State}</td>
            <td>${record.HardwareId}</td>
            <td>${record.DriverId}</td>
            <td>${record.Origin}</td>
            <td>${record.Malfunction}</td>
            <td>${record.Latitude}</td>
            <td>${record.Longitude}</td>
            <td>${record.Odometer}</td>
            <td>${record.EngineHours}</td>
            </tr>
            `;
        });
        const tableBody = `<tbody>${tablerowsHtml}</tbody>`;
        table.innerHTML = tableHead + tableBody;
    }
});
download.addEventListener("click", () => {
    const rows = Array.from(table.querySelectorAll("tr"));
    const csv = rows.map(row => {
        const cells = Array.from(row.querySelectorAll("th, td")).slice(1); // skip the count column             
        return cells.map(cell => `"${(cell.textContent ?? "").replace(/"/g, '""')}"`).join(",");
    }).join("\n");
    const blob = new Blob([csv], { type: "text/csv" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `hos_logs_${currentDatabaseName}_${currentSerialNo}_${fromDate.value}_${toDate.value}.csv`;
    a.click();
    URL.revokeObjectURL(url);
});
function getParameters() {
    const paramFromDate = { Name: "_FROMPARTITIONDATE", Value: fromDate.value };
    const paramToDate = { Name: "_TOPARTITIONDATE", Value: toDate.value };
    const paramDatabasename = { Name: databasename.value === "" ? "Guid" : "DatabaseName", Value: databasename.value === "" ? guid.value : databasename.value };
    const paramDevice = { Name: serialNo.value === "" ? "HardwareId" : "SerialNo", Value: serialNo.value === "" ? hardwareId.value : serialNo.value };
    return [paramFromDate, paramToDate, paramDatabasename, paramDevice];
}
function validateParameters() {
    let validator = true;
    if (databasename.value == "" && guid.value == "") {
        dbError.style.display = 'block';
        validator = false;
    }
    ;
    if (serialNo.value == "" && hardwareId.value == "") {
        deviceError.style.display = 'block';
        validator = false;
    }
    ;
    if (fromDate.value == "" || toDate.value == "") {
        dateError.style.display = 'block';
        validator = false;
    }
    return validator;
}
function mapToDutyStatusLogRecord(row) {
    return {
        Guid: row.companyGuid,
        ParentId: row.parentId,
        Id: row.id,
        DateTime: row.dateTime,
        EditDateTime: row.editDateTime,
        StreamDateTime: row.streamDateTime,
        VerifyDateTime: row.verifyDateTime,
        Version: row.version,
        Sequence: row.sequence,
        Status: row.status,
        HardwareId: row.hardwareId,
        DriverId: row.driverId,
        Origin: row.origin,
        State: row.state,
        Malfunction: row.malfunction,
        Latitude: row.latitude,
        Longitude: row.longitude,
        Odometer: row.odometer,
        EngineHours: row.engineHours,
    };
}
export {};
//# sourceMappingURL=app.js.map