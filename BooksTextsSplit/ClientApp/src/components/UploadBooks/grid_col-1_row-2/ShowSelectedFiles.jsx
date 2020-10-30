import React from "react";
import ShowFiles from "./ShowFiles";
import s from "./ShowSelectedFiles.module.css";

const ShowHeader = () => {
  return (
    <div>
      <div>SELECTED BOOKS FILES PROPERTIES</div>
    </div>
  );
};

const ShowSelectedFiles = (chosenFiles, setRadioResult, radioChosenLanguage, filesDescriptions) => {
  //console.log(chosenFiles);
  return (
    <div className={s.selectedBooksPlace}>
      <div className={s.selectedBooksPlaceItem1}>
        <ShowHeader />
      </div>
      <div className={s.showFilesPlace}>
        <ShowFiles
          chosenFiles={chosenFiles}
          setRadioResult={setRadioResult}
          radioChosenLanguage={radioChosenLanguage}
          filesDescriptions={filesDescriptions}
        />
      </div>
    </div>
  );
};

export default ShowSelectedFiles;
