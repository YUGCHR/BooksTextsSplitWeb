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

/* let bookIds = (allBookNamesSortedByIds, allEngBooksNames, allRusBooksNames) => {
  // debugger;
  //console.log("bookIds started", allBookNamesSortedByIds.length, allBookNamesSortedByIds);
  return (
    /* (
  allEngBooksNames.map((id, i) => {
    console.log("inside bookIds:", id.bookId);
    //debugger;
    return (
      <div>
        <div>
          Fetched {" bookId = " + id.bookId} {" languageId = " + id.languageId} {" uploadVersion = " + id.uploadVersion}
        </div>
        <div>
          Book No: {" " + i + " "} name - {id.authorName + " "} {id.bookName}
        </div>
        <div> ----------------------------------------------------------------------- </div>
      </div>
    ); 
    }));
  allRusBooksNames.map((id, i) => {
    console.log("inside bookIds:", id.bookId);
    //debugger;
    return (
      <div>
        <div>
          Fetched {" bookId = " + id.bookId} {" languageId = " + id.languageId} {" uploadVersion = " + id.uploadVersion}
        </div>
        <div>
          Book No: {" " + i + " "} name - {id.authorName + " "} {id.bookName}
        </div>
        <div> ----------------------------------------------------------------------- </div>
      </div>
    );
  }));
}; */
/*
    allBookNamesSortedByIds.map((id, i) => {
      let placeBooks;
      if (id.languageId === 0) {
        placeBooks = s.fetchedEnglishBooksIds;
      }
      if (id.languageId === 1) {
        placeBooks = s.fetchedRussianBooksIds;
      }
      console.log("inside bookIds:", id.bookId);
      debugger;
      return (
        <div className={placeBooks}>
          <div>
            Fetched {" bookId = " + id.bookId} {" languageId = " + id.languageId} {" uploadVersion = " + id.uploadVersion}
          </div>
          <div>
            Book No: {" " + i + " "} name - {id.authorName + " "} {id.bookName}
          </div>
          <div> ----------------------------------------------------------------------- </div>
        </div>
      );
    })
  );
}; */

let showChooseHeader = () => {
  return <div>CHOOSE BOOKS PAIR BY BookId</div>;
};

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

  console.log("select texts", props);

  return (
    <div className={s.selectPage}>
      <div className={s.pageName}>SELECT BOOKS CONTROL PANEL</div>
      <div className={s.chooseBooks}>{showChooseHeader()}</div>

      {/* <div className={s.flexPlaceBooks}>
        {bookIds(props.allBookNamesSortedByIds, props.allEngBooksNames, props.allRusBooksNames)}
      </div> */}

      <div className={s.showButton}>{/* <button
              onClick={() => showBooksNamesTable()}>SHOW Books IDs</button> */}</div>

      <div></div>
      <div></div>
      <div></div>

      <div className={s.testHeader}>
        <div className={s.testTable}>Header Table</div>
        <div className={s.testItem1}>ENG-01</div>
        <div className={s.testItem2}>RUS-01</div>
        <div className={s.testItem1}>ENG-02</div>
        <div className={s.testItem2}>RUS-02</div>
        <div className={s.testItem1}>ENG-03</div>
        <div className={s.testItem2}>RUS-03</div>
        <div className={s.testItem1}>ENG-04</div>
        <div className={s.testItem2}>RUS-04</div>
        <div className={s.testItem1}>ENG-05</div>
        <div className={s.testItem2}>RUS-05</div>
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
