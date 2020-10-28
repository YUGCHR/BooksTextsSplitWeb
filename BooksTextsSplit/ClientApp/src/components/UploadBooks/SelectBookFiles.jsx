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
    if (e.target.files.length === 2) { //check user selected books pair
      setFileName(e.target.files);
    }
    // TODO if user selected less or more 2 books it is the place  to say him where about this
  };

  const chooseBookFiles = (fileSelectorHandler) => {
    
    return (
      <div>
        <button className={s.allButtonsBodies} onClick={handleClick}>
        Upload Books Pair Files
      </button>
        <input className={s.inputFileNone} type="file" ref={hiddenFileInput} onChange={fileSelectorHandler} multiple />
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
