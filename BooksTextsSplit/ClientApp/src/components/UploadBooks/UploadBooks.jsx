import React from "react";
import s from "./UploadBooks.module.css";

const UploadBooks = (props) => {

  let fileSelectorHandler = (event) => {
    props.setFileName(event.target.files);
  };

  let showSelectedFiles = (chosenFiles) => {
    console.log(chosenFiles);
    if(chosenFiles[0].size) {
    return( 
      Array.from(chosenFiles).map( f => { 
        return ( <div>
          SELECTED FILES - 
          <p>
          <div>File name: {' ' + f.name}</div>
          <div>FLast modified date: {' ' + f.lastModifiedDate}</div>
          <div>File size: {' ' + Math.round(f.size / 1024) + ' KB'}</div>
          </p>
          </div> )
      }));}
  };
 debugger;
  return (
    <div>
      <p className={s.pageName}>UPLOAD BOOKS CONTROL PANEL</p>
      <div>
        <input type="file" onChange={fileSelectorHandler} multiple />
      </div>
      <div>
  <p>
  {showSelectedFiles(props.selectedFiles)}
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
        <p>
          Total records in Cosmos DB =
          {" " + (props.sentencesCount[0] + props.sentencesCount[1])}
        </p>
      </div>
    </div>
  );
};

export default UploadBooks;
