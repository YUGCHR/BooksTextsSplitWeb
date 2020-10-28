import React from "react";
import s from "./sectionDbDetails.module.css";

export const sectionDbDetails = (props) => {
  return (
    <div className={s.showDetailsBody}>
      <div>Sentences count in Cosmos DB</div>
      <div>English sentences count ={" " + props.dbSentencesCount[0]}</div>
      <div>Russian sentences count ={" " + props.dbSentencesCount[1]}</div>
      <div>
        <p>Total records in Cosmos DB ={" " + (props.dbSentencesCount[0] + props.dbSentencesCount[1])}</p>
      </div>
    </div>
  );
};
