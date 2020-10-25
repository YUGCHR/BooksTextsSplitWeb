import React from "react";
import ShowFiles from "./ShowFiles";
import s from "./UploadBooks.module.css";

const ShowHeader = () => {
  return (
    <div>
      <div>SELECTED BOOKS FILES PROPERTIES</div>
    </div>
  );
};

const ShowSelectedFiles = (chosenFiles, setRadioResult, radioChosenLanguage, filesDescriptions) => {
  console.log(chosenFiles);
  return (
    <div className={s.selectedBooksProperties}>
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
