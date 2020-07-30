import React from "react";
import s from "./SelectTexts.module.css";

// TODO rename showSelectButton on showSelectBookIdButton
let showSelectButton = (props, bookId) => {
  console.log("showSelectButton", bookId);
  //debugger;
  return (
    <button
      className={s.testItemButton}
      onClick={() => {
        bookIdSelected(props, bookId);
      }}
    >
      Select {bookId}
    </button>
  );
};

// TODO rename showSelectVersionButton on showSelectVersion(s?)Button(s?)
let showSelectVersionButton = (props, i) => {
  console.log("showSelectButton", i);
  //debugger;
  return (
    <button
      onClick={() => {
        bookVersionSelected(props, i);
      }}
    >
      Select version - {i}
    </button>
  );
};

let bookVersionSelected = (props, i) => {
  props.toggleIsSelectingBookId(false);
  props.toggleIsSelectingUploadVersion(false);
  return { i };
};

let bookIdSelected = (props, bookId) => {
  props.toggleIsSelectingBookId(false);
  props.toggleIsSelectingUploadVersion(true);
  props.fetchAllVersionsOfSelectedBook(bookId);
  return { bookId };
};

// TODO rename on choosePairBooksNames and id on booksNamesSortedById
// TODO change 0, 1 on variable eng and rus (for example)
let bookIds = (props) => {
  if (props.isSelectingBookId) {
    return props.bookNamesVersion1SortedByIds.map((id, i) => {
      let bookId = id.bookId;
      let bookNames = id.booksDescriptions;
      console.log("bookNames", bookNames);
      return (
        <div className={s.testGridContainer3}>
          <div className={s.testItemEng}>{showEngBookName(bookNames[0], bookId, i)}</div>
          <div className={s.testItemRus}>{showEngBookName(bookNames[1], bookId, i)}</div>
          <div className={s.testItemButtonPlace}>{showSelectButton(props, bookId)}</div>
        </div>
      );
    });
  }
};

// TODO rename on showBooksNames
let showEngBookName = (bookName, bookId, i) => {
  return (
    <div className={s.testItemEng}>
      <div>
        Fetched {" bookId = " + bookId} {" languageId = " + bookName.languageId} {" uploadVersion = " + bookName.sentence.uploadVersion}
      </div>
      <div>
        Book No: {" " + i + " "} name - {bookName.sentence.authorName + " "} {bookName.sentence.bookName}
      </div>
    </div>
  );
};

// TODO rename on chooseSelectedBooksVersions and id on BookVersionsDescriptions
// TODO change 0, 1 on variable eng and rus (for example)
let uploadVersions = (props) => {
  if (props.isSelectingUploadVersion) {
    return props.allVersionsOfBooksNames.map((id, i) => {
      return (
        <div>
          <div>Show Version List</div>
        </div>
      );
    });
  }
};

let uploadVersions1 = (props) => {
  if (props.isSelectingUploadVersion) {
    return props.allVersionsOfBooksNames.map((id, i) => {
      let languageId = id.languageId;
      let bookVersions = id.sentences;
      console.log("bookVersions", bookVersions);
      bookVersions.map((sentence, j) => {
        debugger;
        return (
          <div>
            <div>
              {i} Show Version List {j}
            </div>
            <div>{showBookVersions(sentence, languageId, j)}</div>
            <div>{showSelectVersionButton(props, j)}</div>
          </div>
        );
      });
    });
  }
};

// show existing version of the selected book - separate lists for both languages
let showBookVersions = (sentence, languageId, j) => {
  return (
    <div>
      <div>
        Fetched {" j = " + j} {" languageId = " + languageId} {" uploadVersion = " + sentence.uploadVersion}
      </div>
      <div>
        Book No: {" " + j + " "} name - {sentence.authorName + " "} {sentence.bookName}
      </div>
    </div>
  );
};

let showChooseHeader = (props) => {
  if (props.isSelectingBookId) {
    return <div>CHOOSE BOOKS PAIR BY BookId</div>;
  }
  if (props.isSelectingUploadVersion) {
    return <div>CHOOSE UPLOAD VERSION FOR BOOKS PAIR</div>;
  }
};

const SelectTexts = (props) => {
  console.log("select texts", props);

  return (
    <div className={s.testGridContainer1}>
      <div className={s.testItem1}>SELECT BOOKS CONTROL PANEL</div>
      <div className={s.testItem2}>{showChooseHeader(props)}</div>
      <div className={s.testGridContainerPlace2}>
        <div className={s.testGridContainer2}>{bookIds(props)}</div>
        <div className={s.testGridContainer2}>{uploadVersions(props)}</div>
      </div>
    </div>
  );
};

export default SelectTexts;
