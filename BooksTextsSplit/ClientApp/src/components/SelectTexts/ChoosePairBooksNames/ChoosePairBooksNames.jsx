import React from "react";
import s from "./ChoosePairBooksNames.module.css";
//
let showBooksNames = (bookName, languageId, bookId, i) => {
  return (
    <div>
      <div>
        Fetched {" bookId = " + bookId}
        {" languageId = " + languageId}
        {" uploadVersion = "}
      </div>
      <div>
        <div>Book No: {" " + i + " "} name - </div>
        <div>
          {bookName.authorName + " "}{bookName.bookName}
        </div>
      </div>
    </div>
  );
};

let showSelectBookIdButtons = (props, bookId) => {
  //console.log("showSelectBookIdButtons", bookId);
  //debugger;
  return (
    <button
      className={s.testItemButton}
      onClick={() => {
        props.switchBookVersionsOn(bookId);
      }}>
      Select {bookId}
    </button>
  );
};
//booksNamesIds[0].bookDescriptionAllVersions.booksDescriptionsDetails.bookName
const ChoosePairBooksNames = (props) => {
  if (props.isSelectBookId) {
    return props.booksNamesIds.map((id, i) => {
      let bookId = id.bookId;
      let eng = 0;
      let rus = 1;
      let languageId0 = id.bookDescriptionAllVersions[0].languageId;
      let languageId1 = id.bookDescriptionAllVersions[1].languageId;
      let bookNames0 = id.bookDescriptionAllVersions[0].booksDescriptionsDetails[0];
      let bookNames1 = id.bookDescriptionAllVersions[1].booksDescriptionsDetails[0];
      debugger;
      console.log("bookNames", bookNames0);
      console.log("bookNames", bookNames1);
      return (
        <div className={s.testGridContainer3}>
          <div className={s.testItemEng}>{showBooksNames(bookNames0, languageId0, bookId, i)}</div>
          <div className={s.testItemRus}>{showBooksNames(bookNames1, languageId1, bookId, i)}</div>
          <div className={s.testItemButtonPlace}>{showSelectBookIdButtons(props, bookId)}</div>
        </div>
      );
    });
  }
};

export default ChoosePairBooksNames;
