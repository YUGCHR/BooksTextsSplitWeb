import React, { useState } from "react";
import RadioButtons from "./RadioButtons";
import s from "./UploadBooks.module.css";

const ShowHeader = () => {
  return (
    <div>
      <div>SELECTED BOOKS FILES PROPERTIES</div>
    </div>
  );
};

const ShowFiles = ({ chosenFiles, setRadioResult, radioChosenLanguage }) => {
  let newDateFormat = (inputDate) => {
    return new Intl.DateTimeFormat("en-GB", {
      year: "numeric",
      month: "long",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    }).format(inputDate);
  };
  let filesDescriptions = {
    index: "File No: ",
    name: "File name: ",
    lastMod: "Last modified: ",
    size: "File size: ",
    type: "File type: ",
    chosenLanguage: "Chosen file language: ",
  };
  let radioInitialUniqValues = [
    { value: "eng", checked: true, text: "English" },
    { value: "rus", checked: null, text: "Russian" },
    { value: "other", checked: null, text: "User lang" },
  ];
  let radioInitialCommonValues = {
    placeholder: null,
    component: "Input",
    validators: [],
    type: "radio",
    uniqFormName: [], // put (formBaseName + i) here
  };
  let formBaseName = "radioForm";
  //let value="eng";
  return Array.from(chosenFiles).map((f, i) => {
    radioInitialCommonValues.uniqFormName[i] = formBaseName + i;
    console.log("uniqFormName" + i, radioInitialCommonValues.uniqFormName[i]);
    return (
      <div>
        <div>File No: {" " + i} </div>
        <div>File name: {" " + f.name}</div>
        {/* <div>Last modified: {" " + f.lastModifiedDate.toLocaleDateString()}</div> */}
        <div>Last modified: {" " + newDateFormat(f.lastModifiedDate)}</div>
        <div>File size: {" " + Math.round(f.size / 1024) + " KB"}</div>
        <div>File type:{" " + f.type}</div>
        <div className={s.chosenLanguagePlace}>
          <div className={s.chosenLanguageText}>{filesDescriptions.chosenLanguage}</div>
          <div className={s.chosenLanguage}>{" " + radioChosenLanguage[i]}</div>
        </div>
        <div>
          <RadioButtons uV={radioInitialUniqValues} cV={radioInitialCommonValues} i={i} setRadioResult={setRadioResult} />
        </div>
      </div>
    );
  });
};

const ShowSelectedFiles = (chosenFiles, setRadioResult, radioChosenLanguage) => {
  console.log(chosenFiles);

  return (
    <div className={s.selectedBooksProperties}>
      <div className={s.selectedBooksPlaceItem1}>
        <ShowHeader />
      </div>
      <div className={s.showFilesPlace}>
        <ShowFiles chosenFiles={chosenFiles} setRadioResult={setRadioResult} radioChosenLanguage={radioChosenLanguage} />
      </div>
    </div>
  );
};

export default ShowSelectedFiles;
