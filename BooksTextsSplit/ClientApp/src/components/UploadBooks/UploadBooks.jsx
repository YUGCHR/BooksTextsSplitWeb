import React from "react";
import s from "./UploadBooks.module.css";

const UploadBooks = (props) => {

  let fileSelectorHandler = (event) => {
    props.setFileName(event.target.files);
  };

  //  props.handleOptionChange(event.target.value, languageId);

  let handleOptionChange = (event) => {
    props.radioOptionChange(event.target.value, parseInt(event.target.name));
  };

  let createRadioButtons = (radioButtonsValues, i) => {
    //debugger;
    return radioButtonsValues.map((v, j) => {
      return (
        <div className={s.radioBlock}>
          <div className="radio">
            <label>
              <input type="radio" name={i} id={props.radioButtonsIds[(i, j)]} value={v} checked={props.selectedRadioLanguage[i] === v} onChange={handleOptionChange} />
              {props.radioButtonsLabels[j]} {" / languageId = " + props.filesLanguageIds[i]}
            </label>
          </div>
        </div>
      );
    });
  };

  let showSelectedFiles = (chosenFiles) => {
    console.log(chosenFiles);
    //if(chosenFiles[0].size) {
    return Array.from(chosenFiles).map((f, i) => {
      f.languageId = props.filesLanguageIds[i];
      return (
        <div>
          <p>
            <div className={s.eachFileHeader}> File No: {i} </div>
            <div>File name: {" " + f.name}</div>
            <div>FLast modified date: {" " + f.lastModifiedDate}</div>
            <div>File size: {" " + Math.round(f.size / 1024) + " KB"}</div>
            <div>File type: {" " + f.type}</div>
          </p>
          {createRadioButtons(props.radioButtonsValues, i)}
        </div>
      );
    });
  };

  let showFilesToUpload = (chosenFiles) => {
    return Array.from(chosenFiles).map((sf, n) => {
      //debugger;
      return (
        <div>
          <p>
            <div className={s.showFilesToUpload}> File To Upload No: {n} </div>
            <div>File name: {" " + sf.name}</div>
            <div>File language: {" " + sf.languageId}</div>
          </p>
        </div>
      );
    });
  };

  return (
    <div>
      <p className={s.pageName}>UPLOAD BOOKS CONTROL PANEL</p>
      <div>
        <input type="file" onChange={fileSelectorHandler} multiple />
      </div>
      <div>
        <p>
          <div className={s.selectedFiles}>
            <div className={s.selectedHeader}>SELECTED BOOKS FILES -</div>
            <div className={s.selectedFilesTable}>{showSelectedFiles(props.selectedFiles)}</div>
          </div>
        </p>
      </div>
      <div className={s.selectedHeader}>FILES TO UPLOAD -</div>
      <div className={s.showFilesToUpload}>
        <button
          onClick={() => {
            props.fileUploadHandler();
          }}
        >
          UPLOAD SELECTED FILES
        </button>
        <p>
          <p>
            <p>
              <p>
                <p>{showFilesToUpload(props.selectedFiles)}</p>
              </p>
            </p>
          </p>
        </p>
      </div>

      <div>
        <h4>Sentences count in Cosmos DB -</h4>
      </div>
      <div>English sentences count ={" " + props.sentencesCount[0]}</div>
      <div>Russian sentences count ={" " + props.sentencesCount[1]}</div>
      <div>
        <p>Total records in Cosmos DB ={" " + (props.sentencesCount[0] + props.sentencesCount[1])}</p>
      </div>
    </div>
  );
};

export default UploadBooks;
//props.fileUploadHandler(0);

{
  /* <div className={s.radioBlock}>
            <div className="radio">
              <label>
                <input
                  type="radio"
                  name={i}
                  id={props.radioButtonsIds[(i, 0)]}
                  value="1"
                  checked={props.selectedRadioLanguage[i] === "1"}
                  onChange={handleOptionChange}
                />{" "}
                Book with English test
              </label>
            </div>
            <div className="radio">
              <label>
                <input
                  type="radio"
                  name={i}
                  id={props.radioButtonsIds[(i, 1)]}
                  value="2"
                  checked={props.selectedRadioLanguage[i] === "2"}
                  onChange={handleOptionChange}
                />{" "}
                Book with Russian test
              </label>
            </div>
            <div className="radio">
              <label>
                <input
                  type="radio"
                  name={i}
                  id={props.radioButtonsIds[(i, 2)]}
                  value="3"
                  checked={props.selectedRadioLanguage[i] === "3"}
                  onChange={handleOptionChange}
                />{" "}
                I do not know book language
              </label>
            </div>
          </div> */
}
