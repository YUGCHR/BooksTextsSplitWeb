import React from "react";
import cs from "../../../Common.module.css"; // common styles
import s from "./sectionDbInfoHeader.module.css";

export const sectionDbInfoHeader = (props) => {
  return (
    <div className={s.dbInfoButton}>
      <div>{props.uploadBooksLabels.dbInfoHeader}</div>
      <div className={s.dbInfoRecordsCount}>
        {props.isTextLoaded[0] &&
          props.isTextLoaded[1] &&
          props.dbSentencesCount[0].totalRecordsCount +
            props.dbSentencesCount[1].totalRecordsCount +
            props.uploadBooksLabels.nearShowButton}
      </div>
      <button className={cs.allButtonsBodies} onClick={props.setShowHideState}>
        {props.labelShowHide[0].label}
      </button>
    </div>
  );
};
