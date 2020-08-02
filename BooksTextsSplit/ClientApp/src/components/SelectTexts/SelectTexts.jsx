import React from "react";
import s from "./SelectTexts.module.css";

// TODO rename showSelectButton on showSelectBookIdButton
let showSelectButton = (props, bookId) => {
  console.log("showSelectButton", bookId);
  //debugger;
  return (
    <button className={s.testItemButton} onClick={() => { switchBookVersionsOn(props, bookId); }}>Select {bookId}</button>
  );
};

let showVersionsButtons = (props, n, i) => {
  if(n===1){
  return (<div>
    <button className={s.selectVersionButton} onClick={() => { nextAfterBookVersion(props, i); }}>Select {i} for reading</button>
    <button className={s.editVersionButton} onClick={() => { nextAfterBookVersion(props, i); }}>Select {i} for editing</button>
    <button className={s.deleteVersionButton} onClick={() => { nextAfterBookVersion(props, i); }}>Select {i} TO DELETE!</button>
  </div>);
  };
};

//Let to switch on NEXT choosing
let nextAfterBookVersion = (props, i) => {
  props.toggleIsSelectingBookId(false);
  props.toggleIsSelectingUploadVersion(false);
  return { i };
};
//Let to switch on Book Versions choosing
let switchBookVersionsOn = (props, bookId) => {
  props.toggleIsSelectingBookId(false, "");
  props.fetchAllVersionsOfSelectedBook(bookId).then((r) => {
    props.toggleIsSelectingUploadVersion(true, "versionGridContainerPlace");
  });
  return { bookId };
};
//Let to switch on Books Names choosing (return to the previous)
let switchBooksIdsOn = (props) => {  
  props.toggleIsSelectingUploadVersion(false, "");  
  props.fetchAllBookIdsWithNames().then((r) => {
    props.toggleIsSelectingBookId(true, "testGridContainerPlace2");
  });
  return 0;
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
    <div>
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
// Render Book Version - Map by languageId
let chooseSelectedBooksVersions = (props) => {
  if (props.isSelectingUploadVersion) {
    return props.allVersionsOfBooksNames.map((nd, n) => {
      let versionLanguageStyle = `s.versionLanguageStyle${n}`;
      return (<div className={versionLanguageStyle}>
          <div className={s.versionItemsHeader}> All version of books with languageId = {n}</div>
          <div className={s.versionItemsBlock}> {sentencesMap(props, nd, n)}</div>
        </div>
      );
    });
  }
};
// Render Book Version - Map by uploadVersions
let sentencesMap = (props, nd, n) => {
  return nd.sentences.map((id, i) => {
    return (
      <div>
        <div>{showBookVersions(id, i)}</div>
        <div className={s.versionButtonPlace}>
          <div>{showVersionsButtons(props, n, i)}</div>          
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
    return (
      <div className={s.uploadHeaderContainer}>
        <div>CHOOSE UPLOAD VERSION FOR BOOKS PAIR</div>
        <div>
          <button className={s.returnToBookIdButton} onClick={() => { switchBooksIdsOn(props); }}>Return to bookId choosing</button>
        </div>
      </div>
    );
  }
};

const SelectTexts = (props) => {
  console.log("select texts", props);
  
  return (
    <div className={s.testGridContainer1}>
      <div className={s.testItem1}>SELECT BOOKS CONTROL PANEL</div>
      <div className={s.testItem2}>{showChooseHeader(props)}</div>
      <div className={props.gridContainerPlace2}>
        <div className={s.testGridContainer2}>{bookIds(props)}</div>      
        <div className={s.versionsGridContainer}>{chooseSelectedBooksVersions(props)}</div>
      </div>
    </div>
  );
};

export default SelectTexts;
