import React from "react";
import s from "./sectionDbDetails.module.css";
// TODO выводить список книг в таблице с количеством версий каждой
export const sectionDbDetails = (props) => {
  return (
    <div className={s.showDetailsBody}>
      <div>Sentences count in Cosmos DB</div>
      <div>English books count ={" " + props.dbSentencesCount[0].booksIdsCount}</div>
      <div>English versions count ={" " + props.dbSentencesCount[0].versionsCountLanguageId}</div>
      <div>English paragraphs count ={" " + props.dbSentencesCount[0].paragraphsCountLanguageId}</div>
      <div>English sentences count ={" " + props.dbSentencesCount[0].sentencesCountLanguageId}</div>
      <div>-----------------------</div>
      <div>Russian books count ={" " + props.dbSentencesCount[1].booksIdsCount}</div>
      <div>Russian versions count ={" " + props.dbSentencesCount[1].versionsCountLanguageId}</div>
      <div>Russian paragraphs count ={" " + props.dbSentencesCount[1].paragraphsCountLanguageId}</div>
      <div>Russian sentences count ={" " + props.dbSentencesCount[1].sentencesCountLanguageId}</div>
      <div>
        <p>
          Total records in Cosmos DB =
          {" " + (props.dbSentencesCount[0].sentencesCountLanguageId + props.dbSentencesCount[1].sentencesCountLanguageId)}
        </p>
      </div>
    </div>
  );
};

/* dbSentencesCount: 
  {
    booksIdsCount: 0,
    versionsCountLanguageId: 0,
    paragraphsCountLanguageId: 0,
    sentencesCountLanguageId: 0,
    allBooksIdsList: [],
    versionsCountsInBooskIds: [],
    paragraphsCountsInBooskIds: [],
    sentencesCountsInBooskIds: [],    
  }, */
