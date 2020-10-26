import React, { useState } from "react";
import { reduxForm } from "redux-form";
import { createField, Input } from "../common/formControls/FormControls";
import { requiredField } from "../common/validators/Validators";
import RadioButtons from "./RadioButtons";
import s from "./UploadBooks.module.css";

const SelectBookFiles = ({ setFileName }) => {
  const fileSelectorHandler = (e) => {
    //if(chosenFiles[0].size) {
    if (e.target.files.length) {
      setFileName(e.target.files);
    }
  };

  const chooseBookFiles = (fileSelectorHandler) => {
    return (
      <div>
        <input type="file" onChange={fileSelectorHandler} multiple />
      </div>
    );
  };

  return (
    <div className={s.selectFilesBlock}>
      <div className={s.selectFilesHeader}>
        <div>CHOOSE BOOKS FILES (ENG/RUS PAIR)</div>
      </div>
      <div>{chooseBookFiles(fileSelectorHandler)}</div>
    </div>
  );
};

export default SelectBookFiles;
