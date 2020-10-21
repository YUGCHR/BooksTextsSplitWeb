import React, { useState } from "react";
import { reduxForm } from "redux-form";
import { createField, Input } from "../common/formControls/FormControls";
import { requiredField } from "../common/validators/Validators";
import RadioButtons from "./RadioButtons";
import s from "./UploadBooks.module.css";

const ShowHeader = () => {
  return (
    <div>
      <div>SELECTED BOOKS FILES PROPERTIES</div>
    </div>
  );
};

const ShowFiles = ({ chosenFiles, radioInitialUniqValues, radioInitialCommonValues, setRadioResult, radioChosenLanguage }) => {
  let radioBaseName = "files";
  let formName = "radio";
  return Array.from(chosenFiles).map((f, i) => {
    radioInitialCommonValues.name = radioBaseName + i;
    let uniqFormName = formName + i;
    //debugger;
    return (
      <div>
        <div>File No: {" " + i} </div>
        <div>File name: {" " + f.name}</div>
        <div>Last modified: {" " + f.lastModifiedDate.toLocaleDateString()}</div>
        <div>File size: {" " + Math.round(f.size / 1024) + " KB"}</div>
        <div>File type:{" " + f.type}</div>
        <div>Chosen file language:{" " + radioChosenLanguage[i]}</div>
        <div>
          <RadioButtons
            formName={uniqFormName}
            uniqValues={radioInitialUniqValues}
            commonValues={radioInitialCommonValues}
            setRadioResult={setRadioResult}
            index={i}
          />
        </div>
      </div>
    );
  });
};

const ShowSelectedFiles = (chosenFiles, setRadioResult, radioChosenLanguage) => {
  let radioInitialUniqValues = [
    { value: "eng", text: "English" },
    { value: "rus", text: "Russian" },
    { value: "other", text: "User lang" },
  ];
  let radioInitialCommonValues = { placeholder: null, name: "radioBaseName + i", component: Input, validators: [], type: "radio" };

  console.log(chosenFiles);

  return (
    <div className={s.selectedBooksProperties}>
      <div className={s.selectedBooksPlaceItem1}>
        <ShowHeader />
      </div>
      <div className={s.showFilesPlace}>
        <ShowFiles
          chosenFiles={chosenFiles}
          radioInitialUniqValues={radioInitialUniqValues}
          radioInitialCommonValues={radioInitialCommonValues}
          setRadioResult={setRadioResult}
          radioChosenLanguage={radioChosenLanguage}
        />
      </div>
    </div>
  );
};

export default ShowSelectedFiles;
