const updateDelay = 3000;
const relativeErrorCounterMax = 60000 / updateDelay;
let relativeErrorCounter = relativeErrorCounterMax;
let flightsId = [];
let dragEventCounter = 0;

if (typeof $ === 'undefined') {
    alert('jQuery must be included.');
}

$(document).ready(function () {
    $(".drop_box").hide();
    UpdateFlightsTables();
    setInterval(UpdateFlightsTables, updateDelay);

    $("#internalFlightsTable").on("click", ".ibtnDel", DeleteFlightClick);

    $(".flights").on("click", "td[informative]", FlightClick);

    $(".flights").on('drag dragstart dragend dragover dragenter dragleave drop', function (e) {
        // Prevent default behavior (Prevent file from being opened)
        e.preventDefault();
        e.stopPropagation();
    })

    $(".flights").on('dragenter', flightsDragHandler);
    $(".flights").on('dragleave dragend drop', flightsDragEndHandler)
    $(".flights").on('drop', flightsDropHandler);

});

// initialize the map in the map section.
function initMap() {
    const uluru = { lat: -25.363, lng: 131.044 };
    const map = new google.maps.Map(document.getElementById('map'), {
        zoom: 4,
        center: uluru
    });
}

// get flights from database and update the flights tables.
function UpdateFlightsTables() {
    const curUrl = new URL(window.location);
    const relative = curUrl.searchParams.get("relative_to");
    const sync = curUrl.searchParams.get("sync");
    const url = "/api/Flights?relative_to=" + relative + (sync ? "&sync=" + sync : "");

    if (!relative) {
        relativeErrorCounter++;
        if (relativeErrorCounter >= relativeErrorCounterMax) {
            alert("please add relative_to parameter");
            relativeErrorCounter = 0;
        }
        return;
    }

    $.ajax({
        url: url,
        success:
            function (flights) {
                let newFlightsId = [];

                // insert new flights to tables
                for (const flight of flights) {
                    newFlightsId.push(flight.flight_id);
                    const a = $("#" + flight.flight_id);
                    if (!$("#" + flight.flight_id).length) {
                        const flightSource = flight.isExternal ? "external" : "internal";
                        $("#" + flightSource + "FlightsTable tbody").append(FlightToTableRowHTML(flight));
                    }

                    // gal draw route
                }

                // remove deleted flights from tables
                for (const flightId of flightsId) {
                    if (!newFlightsId.includes(flightId)) {
                        // if the row is bold
                        if (flightIsBold(flightId)) {
                            flightUnbold(flightsId);
                        }

                        // remove the flight from table
                        $("#" + flightId).remove();

                        // gal delete route
                    }
                }

                flightsId = newFlightsId;
            },
        error: function (xhr) { alert("Request Error!\nURL: " + url + "\nError: " + xhr.status + " - " + xhr.title); },
    });
}

// add flight according to the given flight plan text.
function AddNewFlightPlan(flightPlanText) {
    if (!IsJson(flightPlanText)) {
        alert("the text is not in a json format");
    }

    const url = "/api/FlightPlan";

    $.ajax({
        type: "POST",
        url: url,
        contentType: "application/json",
        data: flightPlanText,
        success: function (data) {
            alert("file uploaded successfuly");
            UpdateFlightsTables();
        },
        error: function (xhr) { alert("Request Error!\nURL: " + url + "\nError: " + xhr.status + " - " + xhr.title); },
    });

}

// get a flight and return its html table row.
function FlightToTableRowHTML(flight) {
    var newRow = $("<tr id=" + flight.flight_id + ">");
    var cols = "";

    cols += '<td informative>' + flight.flight_id + '</td>';
    cols += '<td informative>' + flight.company_name + '</td>';
    cols += '<td informative>' + flight.passengers + '</td>';

    if (!flight.isExternal) {
        cols += '<td><input type="button" class="ibtnDel btn btn-md btn-danger "  value="X"></td>';
    }

    newRow.append(cols);

    return newRow;
};

// update the flight information in the flight info section.
function UpdateFlightInfo(flightId) {
    const url = "/api/FlightPlan/" + flightId;

    $.ajax({
        type: "GET",
        url: url,
        contentType: "application/json",
        success: function (flightPlan) {
            $("#flight_info_id").html(flightPlan.id);
            $("#flight_info_lon").html(flightPlan.initial_location.longitude);
            $("#flight_info_lat").html(flightPlan.initial_location.latitude);
            $("#flight_info_pas").html(flightPlan.passengers);
            $("#flight_info_com").html(flightPlan.company_name);
            $("#flight_info_time").html(flightPlan.initial_location.date_time);
            $("#flight_info_ext").html(flightPlan.is_external.toString());
        },
        error: function (xhr) { alert("Request Error!\nURL: " + url + "\nError: " + xhr.status + " - " + xhr.title); },
    });
}

function EmptyFlightInfo() {
    $("#flight_info_id").empty();
    $("#flight_info_lon").empty();
    $("#flight_info_lat").empty();
    $("#flight_info_pas").empty();
    $("#flight_info_com").empty();
    $("#flight_info_time").empty();
    $("#flight_info_ext").empty();
}

// handle click on the delete button - delete the chosen flight.
function DeleteFlightClick() {
    const rowToRemove = this.parentElement.parentElement;
    const flightId = rowToRemove.firstChild.textContent;
    const url = "/api/Flights/" + flightId;

    // deleting bolded row?
    if (flightIsBold(flightId)) {
        flightUnbold(flightId);
    }

    $.ajax({
        type: "DELETE",
        url: url,
        success:
            function () {
                rowToRemove.remove();
                alert("flight " + flightId + " was deleted successfuly");
            },
        error: function (xhr) { alert("Request Error!\nURL: " + url + "\nError: " + xhr.status + " - " + xhr.title); },
    });
}

// handle click on a flight in the map or in the table.
function FlightClick() {
    const row = this.parentElement;
    const flightId = row.firstChild.textContent;

    for (const boldedRow of flightsBoldedRows()) {
        boldedRowFlightId = boldedRow.firstChild.textContent;
        flightUnbold(boldedRowFlightId);
    }

    flightBold(flightId);
}


// handle dropped files.
function flightsDropHandler(ev) {
    ev = ev.originalEvent;
    dragEventCounter = 0;
    // Prevent default behavior (Prevent file from being opened)
    ev.preventDefault();
    for (const file of ev.dataTransfer.files) {
        const reader = new FileReader();
        reader.onload = function (evt) {
            AddNewFlightPlan(evt.target.result);
        }
        reader.onerror = function (evt) {
            console.log("error reading file");
        }
        reader.readAsText(file);
    }
}

// handle file dragging over flights tables.
function flightsDragHandler(ev) {
    $(".flights *").css("pointer-events", "none");
    dragEventCounter++;
    $(".internal").hide();
    $(".external").hide();
    $(".drop_box").show();
}

// handle file dragging over flights tables end.
function flightsDragEndHandler(ev) {
    dragEventCounter--;
    if (dragEventCounter === 0) {
        $(".drop_box").hide();
        $(".internal").show();
        $(".external").show();
        $(".flights *").css("pointer-events", "");
    }
}

// bold the flight with id 'flightId'.
function flightBold(flightId) {
    $("#" + flightId).attr("bold", "");
    UpdateFlightInfo(flightId);

    // gal bold route
}

// unbold the flight with id 'flightId'.
function flightUnbold(flightId) {
    if (!flightIsBold(flightId)) {
        return;
    }

    $("#" + flightId).removeAttr("bold");
    EmptyFlightInfo();

    // gal unbold route
}

// return true if the flight with id 'flightId' is bold.
function flightIsBold(flightId) {
    if ($("#" + flightId + "[bold]").length) {
        return true;
    }

    return false;
}

// return the bolded rows.
function flightsBoldedRows() {
    return $("tr[bold]");
}

// return true if the text is in json format, false otherwise.
function IsJson(text) {
    try {
        JSON.parse(text);
    } catch (e) {
        return false;
    }

    return true;
}