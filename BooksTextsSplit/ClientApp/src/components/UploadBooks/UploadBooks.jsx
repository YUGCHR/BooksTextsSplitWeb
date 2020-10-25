import React, { useState } from "react";
import { reduxForm } from "redux-form";
import { createField, Input } from "../common/formControls/FormControls";
import { requiredField } from "../common/validators/Validators";
import RadioButtons from "./RadioButtons";
import SelectBookFiles from "./SelectBookFiles";
import ShowSelectedFiles from "./ShowSelectedFiles";
import s from "./UploadBooks.module.css";

const UploadBooks = ({ selectedFiles, setRadioResult, radioChosenLanguage, filesDescriptions, ...props }) => {
  let showFilesToUpload = (chosenFiles, sentencesCount) => {
    return Array.from(chosenFiles).map((sf, n) => {
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

  return (
    <div>
      <div className={s.allControlPanel}>
        <div className={s.pageName}>
          <div>UPLOAD BOOKS CONTROL PANEL</div>
          <div className={s.selectFiles}>
            <div className={s.selectedBooksPlace}>{!selectedFiles && <SelectBookFiles setFileName={props.setFileName} />}</div>
          </div>
          <div className={s.selectedBooksPlace}>
            {!!selectedFiles && <div>{ShowSelectedFiles(selectedFiles, setRadioResult, radioChosenLanguage, filesDescriptions)}</div>}
          </div>
        </div>

        <div className={s.dbInfoButton}>
          <div>DB INFO</div>
          <div className={s.dbCount}>
            <div>
              <div className={s.dbCountHeader}>Sentences count in Cosmos DB -</div>
            </div>
            <div>English sentences count ={" " + props.dbSentencesCount[0]}</div>
            <div>Russian sentences count ={" " + props.dbSentencesCount[1]}</div>
            <div>
              <p>Total records in Cosmos DB ={" " + (props.dbSentencesCount[0] + props.dbSentencesCount[1])}</p>
            </div>
          </div>
        </div>

        <div className={s.uploadFiles}>
          <div className={s.uploadFilesHeader}>FILES TO UPLOAD -</div>
          <div className={s.showFilesToUpload}>
            <button
              onClick={() => {
                props.fileUploadHandler(props.selectedFiles);
              }}>
              UPLOAD SELECTED FILES
            </button>
            {!!props.selectedFiles && <div>{showFilesToUpload(props.selectedFiles, props.sentencesCount)}</div>}
            {/* <div>{showUploadedSentenceCount()}</div> */}
          </div>
        </div>
      </div>
    </div>
  );
};

export default UploadBooks;
//props.fileUploadHandler(0);
