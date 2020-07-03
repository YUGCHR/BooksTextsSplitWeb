import React from "react";
import s from "./SelectTexts.module.css";

const showSentences = (sentences) => {
  return sentences.map((sts) => {
    return <div /* className={s.oneSentence} */></div>;
  });
};

/* let createDropDownList = (dropDownListValues, i) => {
    //debugger;
    return radioButtonsValues.map((v, j) => {
      return (
        <div className={s.radioBlock}>
          <div className="radio">
            <label>
              <input type="radio" name={i} id={props.radioButtonsIds[(i, j)]} value={v} checked={props.selectedRadioLanguage[i] === v} onChange={handleOptionChange} />
              {props.radioButtonsLabels[j]} {" / languageId = " + props.filesLanguageIds[i]}
            </label>
          </div>
        </div>
      );
    });
  }; */

const SelectTexts = (props) => {
  return (
    <div>
      <div className={s.pageName}>SELECT BOOKS CONTROL PANEL</div>
      <div className={s.chooseBooks}>CHOOSE BOOKS PAIR BY BookId</div>      
      <div className={s.dropDownList}>
       <div>
          <legend>Selecting BookId</legend>
          <div>
             <div>Existing BookId in DB</div>
             <select id = "myList">
               <option value = "1">one</option>
               <option value = "2">two</option>
               <option value = "3">three</option>
               <option value = "4">four</option>
             </select>
          </div>
       </div>
       </div>
      <div>{/* <button onClick={props.fetchSentences}>LOAD ALL</button> */}</div>
      <div></div>
      <div>English sentences</div>
      <div>{showSentences(props.engSentences)}</div>
    </div>
  );
};

export default SelectTexts;
