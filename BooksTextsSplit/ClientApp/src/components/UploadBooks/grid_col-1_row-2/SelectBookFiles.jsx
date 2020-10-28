import React from "react";
import { useRef } from "react";
import cs from ".././UploadBooks.module.css"; // common styles
import s from "./SelectBookFiles.module.css";

const SelectBookFiles = ({ setFileName, isWrongCount }) => {
  const hiddenFileInput = useRef(null);
  const handleClick = (event) => {
    hiddenFileInput.current.click();
  };

  const fileSelectorHandler = (e) => {
    //if(chosenFiles[0].size) {
    if (e.target.files.length) {
      //check user selected any files
      setFileName(e.target.files);
    }
  };

  const chooseBookFiles = (fileSelectorHandler) => {
    return (
      <div>
        <button className={cs.allButtonsBodies} onClick={handleClick}>
          Choose Books Pair Files
        </button>
        <input className={s.inputFileNone} type="file" ref={hiddenFileInput} onChange={fileSelectorHandler} multiple />
        <div className={s.rightFilesCount}>
          {!isWrongCount && "please select books pair - English & Russian"}
          {isWrongCount && <div className={s.wrongFilesCount}>{"you must select 2 books only! (eng/rus pair)"}</div>}
        </div>
      </div>
    );
  };

  return (
    <div>
      <div>
        <div>CHOOSE BOOKS FILES (ENG/RUS PAIR)</div>
      </div>
      <div>{chooseBookFiles(fileSelectorHandler)}</div>
    </div>
  );
};

export default SelectBookFiles;
