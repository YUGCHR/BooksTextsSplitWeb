import React from "react";
import { useRef } from "react";
import s from "./UploadBooks.module.css";

const SelectBookFiles = ({ setFileName }) => {
  const hiddenFileInput = useRef(null);
  const handleClick = event => {
    hiddenFileInput.current.click();
  };
  
  const fileSelectorHandler = (e) => {
    //if(chosenFiles[0].size) {
    if (e.target.files.length) {
      setFileName(e.target.files);
    }
  };

  const chooseBookFiles = (fileSelectorHandler) => {
    
    return (
      <div>
        <button className={s.allButtonsBodies} onClick={handleClick}>
        Upload a file
      </button>
        <input className={s.inputFilePlace} type="file" ref={hiddenFileInput} onChange={fileSelectorHandler} multiple />
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
