import React from "react";
import SelectBookFiles from "./grid_col-1_row-2/SelectBookFiles";
import ShowSelectedFiles from "./grid_col-1_row-2/ShowSelectedFiles";
import { sectionDbInfoHeader } from "./grid_col-2_row-1/sectionDbInfoHeader";
import { sectionDbDetails } from "./grid_col-2_row-2/sectionDbDetails";
import { uploadFiles } from "./grid_col-1_row-3/uploadFiles";
import { ShowFilesToUpload } from "./grid_col-1_row-3/ShowFilesToUpload";
import s from "./UploadBooks.module.css";

const UploadBooks = ({ selectedFiles, setRadioResult, radioChosenLanguage, filesDescriptions, ...props }) => {
  return (
    <div className={s.allControlPanelPlace}>
      {/* Grid Container */}
      <div className={s.allControlPanel}>
        {/* Grid Block col-1 _ row-1 */}
        <div className={s.pageName}>
          {props.uploadBooksLabels.uploadBooksHeader1}
          {props.uploadBooksLabels.uploadBooksHeader2}
        </div>
        {/* Grid Block col-1 _ row-2 */}
        <div className={s.selectFiles}>
          {!selectedFiles && <SelectBookFiles setFileName={props.setFileName} isWrongCount={props.isWrongCount} />}
          {!!selectedFiles && (
            <div>
              {/* TODO add button to return to the files selection? */}
              {/* TODO спрятать после нажатия на кнопку Upload */}
              {ShowSelectedFiles(selectedFiles, setRadioResult, radioChosenLanguage, filesDescriptions)}
            </div>
          )}
        </div>
        {/* Grid Block col-2 _ row-1 */}
        <div className={s.dbInfoButtonPlace}>{sectionDbInfoHeader(props)}</div>
        {/* Grid Block col-2 _ row-2 */}
        <div className={s.showDetailsPlace}>{props.labelShowHide[0].value && <div>{sectionDbDetails(props)}</div>}</div>
        {/* Grid Block col-1 _ row-3 */}
        <div className={s.uploadFiles}>
          {!!selectedFiles && !props.isDoneUpload && uploadFiles(props, selectedFiles)}          
          {props.isDoneUpload && (
            <ShowFilesToUpload key={selectedFiles.languageId} selectedFiles={selectedFiles} sentencesCount={props.sentencesCount} />
          )}
        </div>
        {/* Grid Block col-2 _ row-3 */}
        <div className={s.freeSpaceOnUploadGrid}>{props.taskDonePercents}</div>
      </div>
    </div>
  );
};

export default UploadBooks;
