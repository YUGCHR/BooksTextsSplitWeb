import React from "react";
import ShowChooseHeader from './ShowChooseHeader/ShowChooseHeader';
import ChoosePairBooksNames from './ChoosePairBooksNames/ChoosePairBooksNames';
import ChooseBooksVersions from './ChooseBooksVersions/ChooseBooksVersions';
import QuickViewBooksPair from './QuickViewBooksPair/QuickViewBooksPair';
import s from "./SelectTexts.module.css";

const SelectTexts = (props) => {  
  return (
    <div className={s.testGridContainer1}>
      <div className={s.testItem1}>SELECT BOOKS CONTROL PANEL</div>
      <div className={s.testItem2}>{ShowChooseHeader(props)}</div>
      <div className={s.gridContainerPlace2}>
        <div className={s.gridBooksContainer2}>{ChoosePairBooksNames(props)}</div>      
        <div className={s.gridVersionsContainer2}>{ChooseBooksVersions(props)}</div>
        <div className={s.gridViewContainer}>{QuickViewBooksPair(props)}</div>
      </div>
    </div>
  );
};

export default SelectTexts;
