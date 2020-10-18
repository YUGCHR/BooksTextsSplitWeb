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
              <input
                type="radio"
                name={i}
                id={props.radioButtonsIds[(i, j)]}
                value={v}
                checked={props.selectedRadioLanguage[i] === v}
                onChange={handleOptionChange}
              />
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
      f.bookId = props.booksTitles[i][0].bookId;
      f.authorNameId = props.booksTitles[i][0].authorNameId;
      f.authorName = props.booksTitles[i][0].authorName;
      f.bookNameId = props.booksTitles[i][0].bookNameId;
      f.bookName = props.booksTitles[i][0].bookName;

      return (
        <div className={s.selectedFilesTableRow2}>
          <div> {i} </div>
          <div>{" " + f.name}</div>
          <div>{" " + f.lastModifiedDate}</div>
          <div>{" " + Math.round(f.size / 1024) + " KB"}</div>
          <div>{" " + f.type}</div>
          <div>{createRadioButtons(props.radioButtonsValues, i)}</div>
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
        </div>
        <div className={s.dbInfoButton}>
          <div>DB INFO</div>
        </div>
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
              <div>{showSelectedFiles(props.selectedFiles)}</div>
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
            <div>{showFilesToUpload(props.selectedFiles, props.sentencesCount)}</div>
            {/* <div>{showUploadedSentenceCount()}</div> */}
          </div>
        </div>
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
    </div>
  );
};

export default UploadBooks;
//props.fileUploadHandler(0);
