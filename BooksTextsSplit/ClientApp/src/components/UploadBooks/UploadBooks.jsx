import React from "react";
import s from "./UploadBooks.module.css";

const UploadBooks = (props) => {
  let fileSelectorHandler = (event) => {
    props.setFileName(event.target.files);
  };
  //languageId
  let handleOptionChange = (event) => {    
    props.radioOptionChange(event.target.value);
  };

  let showSelectedFiles = (chosenFiles) => {
    console.log(chosenFiles);
    //if(chosenFiles[0].size) {
    return Array.from(chosenFiles).map((f, i) => {
      return (
        <div>
          <p>
            <div className={s.eachFileHeader}> File No: {i} </div>
            <div>File name: {" " + f.name}</div>
            <div>FLast modified date: {" " + f.lastModifiedDate}</div>
            <div>File size: {" " + Math.round(f.size / 1024) + " KB"}</div>
            <div>File type: {" " + f.type}</div>
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

            <div className={s.radioBlock}>
              <div className="radio">
                <label>
                  <input type="radio" value="1" checked={props.selectedRadioLanguage === '1'} onChange={handleOptionChange} /> Book with English test
                </label>
              </div>
              <div className="radio">
                <label>
                  <input type="radio" value="2" checked={props.selectedRadioLanguage === '2'} onChange={handleOptionChange} /> Book with Russian test
                </label>
              </div>
              <div className="radio">
                <label>
                  <input type="radio" value="3" checked={props.selectedRadioLanguage === '3'} onChange={handleOptionChange} /> I do not know book language
                </label>
              </div>
            </div>

            <div className={s.selectedFilesTable}>{showSelectedFiles(props.selectedFiles)}</div>
          </div>
        </p>
      </div>
      <div>
        <button
          onClick={() => {
            props.fileUploadHandler(0);
          }}
        >
          UPLOAD ENGLISH
        </button>
      </div>
      <div>
        <button
          onClick={() => {
            props.fileUploadHandler(1);
          }}
        >
          UPLOAD RUSSIAN
        </button>
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
