var oeDate;
var isDataChanged = false;
var toSave = true;

//20170502 JPC change to jQuery for triggering initialisation
$(document).ready(XOnstart);

function XOnstart() {
    //alert("function XOnstart is starting");

    //Date may look like this "28/03/2017 12:00:00 AM"
    // - need to simplify to only the date like "28/03/2017"
    oeDate = document.getElementById("Date");
    var sDate = oeDate.value;
    oeDate.value = sDate.split(" ")[0];
    XDisplayDay();

    for (var i = 1; i <= 2; i++) {
        var sDateTime = document.getElementById("Time" + i).value;
        if (IsDate(sDateTime)) {
            //20170502 JPC sDateTime like 2/05/2017 needs leading zero like 02/05/2017
            if (sDateTime.indexOf("/") == 1) sDateTime = "0" + sDateTime;
            //20170326 JPC bug fix XDisplayDateTime needs Date object not string
            // AND some JavaScript date functions need conversion to ISO format to work
            var sDateSpaceArray = sDateTime.split(" ");
            var sDateOnly = sDateSpaceArray[0];
            //20170502 JPC TL change values like "6:00:00" to "06:00:00"
            if(sDateSpaceArray[1].indexOf(":") == 1) sDateSpaceArray[1] = "0" + sDateSpaceArray[1];
            //Change to ISO format including 24 hour clock
            if (sDateSpaceArray.length > 2) {
                var sTimeArray = sDateSpaceArray[1].split(":");
                if(sDateSpaceArray[2].toUpperCase().slice(0, 1) == "P") {
                    //if statement to allow for case of 12 p.m. as an exception
                    if (sTimeArray[0] != "12") {
                        sTimeArray[0] = (parseInt(sTimeArray[0]) + 12).toString();
                    }
                } else {
                    //we have a.m and we need to allow for case of 12 a.m
                    if (sTimeArray[0] == "12") {
                        sTimeArray[0] = "00";
                    }
                }
                sDateSpaceArray[1] = sTimeArray.join(":");
            }

            //Humpty Dumpty fell off a wall
            sDateOnlyArray = sDateOnly.split("/");
            //But we can put Humpty Dumpty together again
            var sDateISO = sDateOnlyArray[2] + "-" + sDateOnlyArray[1] + "-" + sDateOnlyArray[0] + "T" + sDateSpaceArray[1];
            //20170502 JPC TL
            //alert("sDateTime = " + sDateTime + " and sDateISO = " + sDateISO);
            var d = new Date(sDateISO);
            XDisplayDateTime(d, i);
        }
    }
}

function XDisplayDay() {
    //When user edits the date field, change the day-of-week display to match
    //Check that current input is a date
    var sDate = oeDate.value;
    if (!IsDate(sDate)) return;
    sDate = sDate.split(" ")[0];
    //Need to manually change date format to US for this function getDay() to work
    //20170502 JPC EDUC next line was missing semicolon at end - did run on IE and some Google Chrome
    var sPart = sDate.split("/");
    var d = new Date(sPart[1] + "/" + sPart[0] + "/" + sPart[2]);
    var s = "";
    switch (d.getDay()) {
        case 0: s = "Sun"; break;
        case 1: s = "Mon"; break;
        case 2: s = "Tue"; break;
        case 3: s = "Wed"; break;
        case 4: s = "Thu"; break;
        case 5: s = "Fri"; break;
        case 6: s = "Sat"; break;
    }
    document.getElementById("txtDayOfWeek").value = s;
}

//Reference
//Olsen, N. (2011). JavaScript Detecting Valid Dates. Retrieved From http://jsfiddle.net/zKb6c/
function IsDate(sDate) {
    sDate = sDate.split(" ")[0];
    //20170326 JPC having problems with jQuery effect on Visual Studio 2017 debugging
    // therefore change from $.trim() to sDate.trim()
    if (sDate.trim() == "") return false;
    var dateArray = sDate.split("/");
    var dd = parseInt(dateArray[0], 10);
    var MM = parseInt(dateArray[1], 10);
    var yyyy = parseInt(dateArray[2], 10);
    var date = new Date(yyyy, MM - 1, dd);
    if (date.getFullYear() == yyyy && date.getMonth() + 1 == MM && date.getDate() == dd) {
        return true;
    } else {
        return false;
    }
}


function XOnSubmit() {
    //20170325 JPC update hidden field from visible dropdowns
    for (var i = 1; i <= 2; i++) {
        document.getElementById("Time" + i).value
            = document.getElementById("txtTime" + i).value.trim()
            + " " + document.getElementById("ddlHours" + i).value
            + ":" + document.getElementById("ddlMinutes" + i).value;
    }

    //Oversimplified validation for now
    if (!IsDate(document.getElementById("Date").value)) {
        alert("ERROR: Needs a valid Date");
        document.getElementById("Date").focus();
        return false;
    }
    var oeTime1 = document.getElementById("Time1");
    if (!IsDate(oeTime1.value)) {
        alert("ERROR: Needs a valid Time1");
        oeTime1.focus();
        return false;
    }
    var oeTime2 = document.getElementById("Time2")
    if (!IsDate(oeTime2.value)) {
        alert("ERROR: Needs a valid Time2");
        oeTime2.focus();
        return false;
    }

    //Check for  not timeDate2 < timeDate1 ie we are not working negative hours
    var sTimeDate1 = oeTime1.value.trim();
    var sDT1 = sTimeDate1.split(" ");
    var sTimeDate1Array = sDT1[0].split("/");
    var timeDate1 = new Date(sTimeDate1Array[1] + "/" + sTimeDate1Array[0] + "/" + sTimeDate1Array[2] + " " + sDT1[1]);
    var sTimeDate2 = oeTime2.value.trim();
    var sDT2 = sTimeDate2.split(" ");
    var sTimeDate2Array = sDT2[0].split("/");
    var timeDate2 = new Date(sTimeDate2Array[1] + "/" + sTimeDate2Array[0] + "/" + sTimeDate2Array[2] + " " + sDT2[1]);
    if (timeDate2 < timeDate1) {
        alert("ERROR: Time2 cannot be earlier than Time1. Have you crossed midnight? If yes then you may need to click the 'Plus-Or-Minus' button to move into the next day.")
        oeTime2.focus();
        return false;
    }
    
    //Call function to give the "escape" treatment to HTML code input
    XNotesUpdate();
    return true;
}

//Get Date and Time NOW from the client system for Time1 or Time2 auto data entry
//code from http://www.w3schools.com/jsref/jsref_gethours.asp
//with some extra coding work to be compatible with the string values in the proposed dropdowns.
//.slice(-2) means extract the rightmost 2 characters of a string
function XNow(i) {
    var date = new Date();
    XDisplayDateTime(date, i);
}

function XDisplayDateTime(date, i) {
    var yyyy = date.getFullYear();
    //"The getMonth() method returns the month (from 0 to 11) for the specified date, according to local time. 
    //Note: January is 0, February is 1, and so on." - https://www.w3schools.com/jsref/jsref_getmonth.asp
    //Therefore we need a + 1 in the next line.
    var MM = ("00" + (date.getMonth() + 1).toString()).slice(-2);
    //getDate() returns day of the month 1-31
    var dd = ("00" + (date.getDate()).toString()).slice(-2); 
    var hh = ("00" + (date.getHours()).toString()).slice(-2);
    //minutes to nearest 0, 15, 30, 45
    var mm = ("00" + ((Math.round(date.getMinutes() / 15) * 15).toString())).slice(-2);
    if (mm == "60") {
        mm = "00";
        hh = ("00" + (parseInt(h) + 1).toString()).slice(-2);
        if (hh == 24) {
            hh = "00";
            alert("Midnight - Check Date of Time" + i + ".  You may need to increase it by one day");
        }
    }
    //
    //20170502 JPC EDUC next line was missing semicolon at end - did run on IE and some Google Chrome
    document.getElementById("txtTime" + i).value = dd + "/" + MM + "/" + yyyy;
    document.getElementById("ddlHours" + i).value = hh;
    document.getElementById("ddlMinutes" + i).value = mm;
}

function XDatePlusMinus(i) {
    //If timedate is the same as date then add a day to timedate
    //Else revert timedate back to date
    //Mainly useful for cross-midnight data entry
    sTimeDate = document.getElementById("txtTime" + i).value;
    sDate = document.getElementById("Date").value;
    if (sTimeDate != sDate) {
        sTimeDate = sDate;
    } else {
        //Now gets interesting as we increase the day by 1
        sTimeDateArray = sTimeDate.split("/");
        sTimeDateUnitedStatesFormat = sTimeDateArray[1] + "/" + sTimeDateArray[0] + "/" + sTimeDateArray[2];
        var date = new Date(sTimeDateUnitedStatesFormat);
        date.setDate(date.getDate() + 1);
        var dd = ("00" + (date.getDate()).toString()).slice(-2); //day of the month 1-31
        var MM = ("00" + (date.getMonth() + 1).toString()).slice(-2);
        var yyyy = date.getFullYear();
        sTimeDate = dd + "/" + MM + "/" + yyyy;
    }
    document.getElementById("txtTime" + i).value = sTimeDate;
}


function XNotesUpdate() {
    //20170303 JPC experiment workaround for tinyMCE escape HTML input
    //var s = tinymce.activeEditor.getContent();
    var s = document.getElementById("XNotes").value;
    //Note that JavaScript replace function needs regular expression format with /target/g
    //Ref: https://www.w3schools.com/jsref/jsref_replace.asp
    s = s.replace(/>/g, "&xgt;");
    document.getElementById("Notes").value = s.replace(/</g, "&xlt;");
    //alert(s);
}

//20170502 JPC currently inactive - TODO make use of this when cancelling out after data changed
function XCheck() {
    if (!isDataChanged) {
        return;
    } 

    toSave = confirm("You have made changes.  Do you want to save these changes?");

    // $dialog.dialog("open");
    if (toSave) {
        document.getElementById("hidToSave").value = "true";
    } else {
        document.getElementById("hidToSave").value = "false";
    }
}