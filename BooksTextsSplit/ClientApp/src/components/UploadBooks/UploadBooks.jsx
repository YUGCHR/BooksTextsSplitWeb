import React, { useState } from "react";
import { reduxForm } from "redux-form";
import { createField, Input } from "../common/formControls/FormControls";
import { requiredField } from "../common/validators/Validators";
import RadioButtons from "./RadioButtons";
import s from "./UploadBooks.module.css";

const UploadBooks = (props) => {
  let radioInitialUniqValues = [
    { value: "eng", text: "English" },
    { value: "rus", text: "Russian" },
    { value: "other", text: "User lang" },
  ];
  let radioInitialCommonValues = { placeholder: null, name: "radioBaseName + i", component: Input, validators: [], type: "radio" };
  let radioBaseName = "files";
  let formName = "radio";

  let showSelectedFiles = (chosenFiles) => {
    console.log(chosenFiles);
    //if(chosenFiles[0].size) {
    return Array.from(chosenFiles).map((f, i) => {
      radioInitialCommonValues.name = radioBaseName + i;
      let uniqFormName = formName + i;
      debugger;
      return (
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
      );
    });
  };

  let radioButtonsValues = ["1", "2", "3"];
  let radioButtonsIds = [
    ["eng1", "eng2", "eng3"],
    ["rus1", "rus2", "rus3"],
  ];
  let radioButtonsLabels = ["Book with English test", "Book with Russian test", "I do not know book language"];
  let radioButtonsNames = ["radioEnglish", "radioRussian"];
  const [selectedRadioLanguage, setSelectedRadioLanguage] = useState(["1", "2"]);
  const [filesLanguageIds, setFilesLanguageIds] = useState([0, 1]);

  let fileSelectorHandler = (e) => {
    if (e.target.files.length) {
      props.setFileName(e.target.files);
    }
  };

  //  props.handleOptionChange(event.target.value, languageId);

  let handleOptionChange = (event) => {
    let option = event.target.value;
    let i = parseInt(event.target.name);
    let newSelectedRadioLanguage = [];
    selectedRadioLanguage.map((lang, n) => {
      newSelectedRadioLanguage[n] = lang;
      if (n === i) {
        newSelectedRadioLanguage[n] = option;
      }
    });
    setSelectedRadioLanguage(newSelectedRadioLanguage);
    let languageId = i - 1;

    let newFilesLanguageIds = [];
    filesLanguageIds.map((lang, n) => {
      newFilesLanguageIds[n] = lang;
      if (n === i) {
        newFilesLanguageIds[n] = languageId;
      }
    });
    setFilesLanguageIds(newFilesLanguageIds);
    //props.radioOptionChange(event.target.value, parseInt(event.target.name));
    // const radioOptionChange = (option, i) => ({ type: RADIO_IS_CHANGED, option, i });
  };

  /* export const radioOptionChange = (option, i) => ({ type: RADIO_IS_CHANGED, option, i });
  case RADIO_IS_CHANGED: {
    //debugger;
    let stateCopy = { ...state };
    stateCopy.selectedRadioLanguage = { ...state.selectedRadioLanguage };
    stateCopy.selectedRadioLanguage[action.i] = action.option;
    stateCopy.filesLanguageIds = { ...state.filesLanguageIds };
    let languageId = parseInt(action.option) - 1;
    stateCopy.filesLanguageIds[action.i] = languageId;
    return stateCopy;
    //return { ...state, selectedRadioLanguage[action.languageId]: action.option };
  } */

  let createRadioButtons = (i) => {
    //debugger;
    return radioButtonsValues.map((v, j) => {
      return (
        <div className={s.radioBlock}>
          <div className="radio">
            <label>
              <input
                type="radio"
                name={i}
                id={radioButtonsIds[(i, j)]}
                value={v}
                checked={selectedRadioLanguage[i] === v}
                onChange={handleOptionChange}
              />
              {radioButtonsLabels[j]} {" / languageId = " + filesLanguageIds[i]}
            </label>
          </div>
        </div>
      );
    });
  };

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
            <div className={s.selectFilesBlock}>
              <div className={s.selectFilesHeader}>
                <div>CHOOSE BOOKS FILES (ENG/RUS PAIR)</div>
              </div>
              <div className={s.selectFilesButton}>
                <input type="file" onChange={fileSelectorHandler} multiple />
              </div>
              <div className={s.selectedBooksPlace}>
                <div className={s.selectedBooksContainer}>
                  <div className={s.selectedBooksHeader}>SELECTED BOOKS FILES -</div>
                </div>
                <div className={s.selectedFilesTableRow1}>
                  <div>File No: </div>
                  <div>File name: </div>
                  <div>Last modified: </div>
                  <div>File size: </div>
                  <div>Choose file language: </div>
                </div>

                {/* <div className={s.selectedFilesTableFlexColumns}> */}
                {!!props.selectedFiles && <div>{showSelectedFiles(props.selectedFiles)}</div>}
              </div>
            </div>
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
