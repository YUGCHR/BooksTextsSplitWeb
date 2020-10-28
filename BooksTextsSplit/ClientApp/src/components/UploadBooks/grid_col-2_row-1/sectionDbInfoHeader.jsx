import React from "react";
import cs from ".././UploadBooks.module.css"; // common styles
import s from "./sectionDbInfoHeader.module.css";

export const sectionDbInfoHeader = (props) => {
  return (
    <div className={s.dbInfoButton}>
      <div>{props.uploadBooksLabels.dbInfoHeader}</div>
      <div className={s.dbInfoRecordsCount}>
        {props.dbSentencesCount[0] + props.dbSentencesCount[1] + props.uploadBooksLabels.nearShowButton}
      </div>
      <button className={cs.allButtonsBodies} onClick={props.setShowHideState}>
        {props.labelShowHide[0].label}
      </button>
    </div>
  );
};
