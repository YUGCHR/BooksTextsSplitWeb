import React from "react";
import s from "./SelectTexts.module.css";

/* let createBooksNames = (props) => {
  return props.allBookIdsWithNames.map((id) => {
    debugger;
    return (
      <div>
        <div>bookId = {id}</div>
      </div>
    );
  });
}; */

let bookIds = (arrr) => {  
 // debugger;
  console.log('bookids started', arrr.length, arrr);
  return arrr.map((id) => {
    console.log('inside bookids:', id);
    //debugger;
    return <div>Fetched book Id = {id}</div>;
  });
};



let showChooseHeader = () => {
  return <div>CHOOSE BOOKS PAIR BY BookId</div>
}

/* let createBooksNamesTable = (props) => {
props.fetchAllBookIdsWithNames().then((s) => {
  createBooksNames(props);
  return s;
});
}; */

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

  /* let showBooksNamesTable = () => {  
    createBooksNamesTable();
  };
   */

  /* let showBooksNamesTable = () => {
    return bookIds(props.allBookIdsWithNames);
  } */

  console.log('select texts', props);

  return (
    <div className={s.selectPage}>
      <div className={s.pageName}>SELECT BOOKS CONTROL PANEL</div>
      <div className={s.chooseBooks}>{showChooseHeader()}</div>   
      <div className={s.fetchedBooksIds}>{bookIds(props.allBookIdsWithNames)}</div>   
      <div className={s.showButton}>
        {/* <button
              onClick={() => showBooksNamesTable()}>SHOW Books IDs</button> */}
      </div>
      
    </div>
  );
};

export default SelectTexts;

/* <div className={s.dropDownList}>
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
       </div> */
