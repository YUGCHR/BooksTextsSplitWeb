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

let showSelectVersionButton = (props, i) => {
  return (
    <button
      onClick={() => {
        bookVersionSelected(props, i);
      }}
    >
      Select version {i} for reading
    </button>
  );
};

let showEditVersionButton = (props, i) => {
  return (
    <button
      onClick={() => {
        bookVersionSelected(props, i);
      }}
    >
      Select version {i} for editing
    </button>
  );
};

let showDeleteVersionButton = (props, i) => {
  return (
    <button
      onClick={() => {
        bookVersionSelected(props, i);
      }}
    >
      Select version {i} TO DELETE!
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

  props.fetchAllVersionsOfSelectedBook(bookId).then((r) => {
    props.toggleIsSelectingUploadVersion(true);
  });
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
          <div className={s.testItemEng}>{showBooksNames(bookNames[0], bookId, i)}</div>
          <div className={s.testItemRus}>{showBooksNames(bookNames[1], bookId, i)}</div>
          <div className={s.testItemButtonPlace}>{showSelectButton(props, bookId)}</div>
        </div>
      );
    });
  }
};

let showBooksNames = (bookName, bookId, i) => {
  return (
    <div className={s.versionItemText}>
      <div>
        Fetched {" bookId = " + bookId} {" languageId = " + bookName.languageId} {" uploadVersion = " + bookName.sentence.uploadVersion}
      </div>
      <div>
        Book No: {" " + i + " "} name - {bookName.sentence.authorName + " "} {bookName.sentence.bookName}
      </div>
    </div>
  );
};

// TODO rename nd/id on something
let chooseSelectedBooksVersions = (props) => {
  if (props.isSelectingUploadVersion) {
    return props.allVersionsOfBooksNames.map((nd, n) => {
      let versionLanguageStyle = "s.versionLanguageStyle" + n;
      return (
        <div className={versionLanguageStyle}>
          <div> All version of books with languageId = {n}</div>
          <div className={s.versionItemsBlock}> {sentencesMap(props, nd, n)}</div>
        </div>
      );
    });
  }
};

let sentencesMap = (props, nd, n) => {
  return nd.sentences.map((id, i) => {
    return (
      <div>
        <div>{showBookVersions(id, i)}</div>
        <div className={s.versionButtonPlace}>
          <div>{showSelectVersionButton(props, i)}</div>
          <div>{showEditVersionButton(props, i)}</div>
          <div>{showDeleteVersionButton(props, i)}</div>
        </div>
      </div>
    );
  });
};

// show existing version of the selected book - separate lists for both languages
let showBookVersions = (id, j) => {
  //debugger;
  return (
    <div className={s.versionItemText}>
      <div>
        Fetched {" j = " + j} {" uploadVersion = " + id.uploadVersion}
      </div>
      <div>
        Book No: {" " + j + " "} name - {id.authorName + " "} {id.bookName}
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
        <div className={s.versionGridContainer}>{chooseSelectedBooksVersions(props)}</div>
      </div>
    </div>
  );
};

export default SelectTexts;
