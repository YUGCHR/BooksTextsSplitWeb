import React, { useState } from "react";
import { reduxForm } from "redux-form";
import { createField, Input } from "../common/formControls/FormControls";
import { requiredField } from "../common/validators/Validators";
import RadioButtons from "./RadioButtons";
import s from "./UploadBooks.module.css";

const ShowHeader = () => {
  return (
    <div className={s.selectedBooksContainer}>
      <div className={s.selectedBooksHeader}>SELECTED BOOKS FILES -</div>
      <div className={s.selectedFilesTableRow1}>
        <div>File No: </div>
        <div>File name: </div>
        <div>Last modified: </div>
        <div>File size: </div>
        <div>Choose file language: </div>
      </div>
    </div>
  );
};

const ShowFiles = ({ chosenFiles, radioInitialUniqValues, radioInitialCommonValues }) => {
  let radioBaseName = "files";
  let formName = "radio";
  return Array.from(chosenFiles).map((f, i) => {
    radioInitialCommonValues.name = radioBaseName + i;
    let uniqFormName = formName + i;
    debugger;
    return (
      <div className={s.selectedBooksPlace}>
        <div className={s.selectedFilesTableRow2}>
          <div> {i} </div>
          <div>{" " + f.name}</div>
          <div>{" " + f.lastModifiedDate}</div>
          <div>{" " + Math.round(f.size / 1024) + " KB"}</div>
          <div>{" " + f.type}</div>
          <div>
            <RadioButtons formName={uniqFormName} uniqValues={radioInitialUniqValues} commonValues={radioInitialCommonValues} />
          </div>
        </div>
      </div>
    );
  });
};

const ShowSelectedFiles = (chosenFiles) => {
  let radioInitialUniqValues = [
    { value: "eng", text: "English" },
    { value: "rus", text: "Russian" },
    { value: "other", text: "User lang" },
  ];
  let radioInitialCommonValues = { placeholder: null, name: "radioBaseName + i", component: Input, validators: [], type: "radio" };

  console.log(chosenFiles);

  return (
    <div>
      <ShowHeader />
      <ShowFiles chosenFiles={chosenFiles} radioInitialUniqValues={radioInitialUniqValues} radioInitialCommonValues={radioInitialCommonValues} />
    </div>
  );
};

export default ShowSelectedFiles;
