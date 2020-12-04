import React from 'react';
import s from './ChoosePairBooksNames.module.css';

let showBooksNames = (bookName, bookId, i) => {
  return (
    <div>
      <div>
        Fetched {" bookId = " + bookId} {" languageId = " + bookName.languageId} {" uploadVersion = " + bookName.sentence.uploadVersion}
      </div>
      <div>
        <div>Book No: {" " + i + " "} name - </div>
        <div>{bookName.sentence.authorName + " "} {bookName.sentence.bookName}</div>
      </div>
    </div>
  );
};

let showSelectBookIdButtons = (props, bookId) => {
  //console.log("showSelectBookIdButtons", bookId);
  //debugger;
  return (
    <button className={s.testItemButton} onClick={() => { props.switchBookVersionsOn(bookId); }}>Select {bookId}</button>
  );
};

const ChoosePairBooksNames = (props) => {
  if (props.isSelectBookId) {
    return props.booksNamesIds.map((id, i) => {
      let bookId = id.bookId;
      let bookNames = id.booksDescriptions;
      let eng = 0;
      let rus = 1;
      //console.log("bookNames", bookNames);
      return (        
        <div className={s.testGridContainer3}>
          <div className={s.testItemEng}>{showBooksNames(bookNames[eng], bookId, i)}</div>
          <div className={s.testItemRus}>{showBooksNames(bookNames[rus], bookId, i)}</div>
          <div className={s.testItemButtonPlace}>{showSelectBookIdButtons(props, bookId)}</div>
        </div>       
      );
    });
  }
};

export default ChoosePairBooksNames;
  