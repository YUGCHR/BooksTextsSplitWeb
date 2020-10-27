import React, { useState } from "react";
import { reduxForm } from "redux-form";
import { createField, Input } from "../common/formControls/FormControls";
import { requiredField } from "../common/validators/Validators";
import RadioButtons from "./RadioButtons";
import SelectBookFiles from "./SelectBookFiles";
import ShowSelectedFiles from "./ShowSelectedFiles";
import s from "./UploadBooks.module.css";

const sectionDbInfoHeader = (props) => {
  return (
    <div className={s.dbInfoButton}>
      <div>{"DB INFO"}</div>
      <div className={s.dbInfoRecordsCount}>{ (props.dbSentencesCount[0] + props.dbSentencesCount[1]) + " records, details - "}</div>
      <button className={s.allButtonsBodies} onClick={props.setShowHideState}>
        {props.labelShowHide[0].label}
      </button>
    </div>
  );
};

const sectionDbDetails = (props) => {
  return (
    <div className={s.dbCountHeader}>
      <div>Sentences count in Cosmos DB</div>
      <div>English sentences count ={" " + props.dbSentencesCount[0]}</div>
      <div>Russian sentences count ={" " + props.dbSentencesCount[1]}</div>
      <div>
        <p>Total records in Cosmos DB ={" " + (props.dbSentencesCount[0] + props.dbSentencesCount[1])}</p>
      </div>
    </div>
  );
};

const ShowFilesToUpload = ({ selectedFiles, sentencesCount }) => {
  return Array.from(selectedFiles).map((sf, n) => {
    //debugger;
    return (
      <div>
        <p>
          <div className={s.showFilesToUpload}> File To Upload No: {n} </div>
          <div>File name: {" " + sf.name}</div>
          <div>File language: {" " + sf.languageId}</div>
          <div>Uploaded sentences count: {" " + sentencesCount[n]}</div>
        </p>
      </div>
    );
  });
};

const UploadBooks = ({ selectedFiles, setRadioResult, radioChosenLanguage, filesDescriptions, ...props }) => {
  return (
    <div className={s.allControlPanelPlace}>
      <div className={s.allControlPanel}>
        <div className={s.pageName}>{props.uploadBooksLabels.uploadBooksHeader1}{props.uploadBooksLabels.uploadBooksHeader2}</div>
        <div className={s.selectFiles}>
          {!selectedFiles && <SelectBookFiles setFileName={props.setFileName} />}
          {!!selectedFiles && (
            <div className={s.selectedBooksPlace}>
              {ShowSelectedFiles(selectedFiles, setRadioResult, radioChosenLanguage, filesDescriptions)}
            </div>
          )}
        </div>
        <div className={s.dbInfoButtonPlace}>{sectionDbInfoHeader(props)}</div>
        {props.labelShowHide[0].value && <div className={s.showDetailsPlace}>{sectionDbDetails(props)}</div>}
        <div className={s.uploadFiles}>
          {!!selectedFiles && (
            <div>
              <div className={s.uploadFilesHeader}>If files are ready, please</div>
              <div className={s.showFilesToUpload}>
                <button
                  className={s.allButtonsBodies}
                  onClick={() => {
                    props.fileUploadHandler(selectedFiles);
                  }}>
                  {props.uploadBooksLabels.uploadButton}
                </button>
              </div>
            </div>
          )}
          {/* TODO change selectedFiles on new selector (named uploadStartState or the same), change it after button UPLOAD was pressed */}
          {!!selectedFiles && <ShowFilesToUpload selectedFiles={selectedFiles} sentencesCount={props.sentencesCount} />}
          {/* <div>{showUploadedSentenceCount()}</div> */}
        </div>
      </div>
    </div>
  );
};

export default UploadBooks;
//props.fileUploadHandler(0);
