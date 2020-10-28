import React from "react";
import cs from ".././UploadBooks.module.css";
import s from "./uploadFiles.module.css";

export const uploadFiles = (props, selectedFiles) => {
  return (
    <div className={s.uploadFilesLabelButtonPlace}>
      <div className={s.uploadFilesLabel}>If files are ready, please</div>
      <button
        className={cs.allButtonsBodies}
        disabled={props.isUploadButtonDisabled}
        onClick={() => {
          props.fileUploadHandler(selectedFiles);
        }}>
        {props.uploadBooksLabels.uploadButton}
      </button>
    </div>
  );
};
