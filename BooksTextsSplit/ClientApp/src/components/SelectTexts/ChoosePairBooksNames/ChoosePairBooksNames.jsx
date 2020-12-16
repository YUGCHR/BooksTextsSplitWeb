import React from "react";
import s from "./ChoosePairBooksNames.module.css";
//
let showBooksNames = (bookDescription, bookId, i) => {
  let languageId = bookDescription.languageId;
  let bookName = bookDescription.bookVersionsInLanguage[0].bookDescriptionDetails;
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
          {bookName.authorName + " "}
          {bookName.bookName}
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
      let bookDescription = id.bookVersionsLanguageInBook;
      return bookDescription.map((l, j) => {
        return (
          <div className={s.testGridContainer3}>
            <div className={s.testItemEng}>{showBooksNames(l, bookId, i)}</div>
            <div className={s.testItemButtonPlace}>{showSelectBookIdButtons(props, bookId)}</div>
          </div>
        );
      });
    });
  }
};

export default ChoosePairBooksNames;
