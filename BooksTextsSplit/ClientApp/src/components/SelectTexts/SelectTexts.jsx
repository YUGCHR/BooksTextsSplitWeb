import React from "react";
import s from "./SelectTexts.module.css";

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

let bookIdSelected = (props, bookId) => {
  props.toggleIsSelectingBookId(false);
  props.toggleIsSelectingUploadVersion(true);
  return { bookId };
};

let bookIds = (props) => {
  if (props.isSelectingBookId) {
    return props.version1BookNamesSortedByIds.map((id, i) => {
      let bookId = id.bookId;
      let bookNames = id.bookNames;
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

let uploadVersions = (props) => {
  if (props.isSelectingUploadVersion) {
    return props.version1BookNamesSortedByIds.map((id, i) => {
      let bookId = id.bookId;
      let bookNames = id.bookNames;
      console.log("bookNames", bookNames);

      return (
        <div className={s.testGridContainer3}>uploadVersions here
          <div className={s.testItemEng}>{showEngBookName(bookNames[0], bookId, i)}</div>
          <div className={s.testItemRus}>{showEngBookName(bookNames[1], bookId, i)}</div>
          <div className={s.testItemButtonPlace}>{showSelectButton(props, bookId)}</div>
        </div>
      );
    });
  }
};

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
