import React from 'react';
import s from './ChooseBooksVersions.module.css';

// Render Book Version - Map by uploadVersions
let sentencesMap = (props, nd, n) => {
  return nd.sentences.map((id, i) => {
    let versionSentencesPlace = `versionSentencesPlace${n}`;
    return (      
      <div className={s.versionItemsBlock1Container3}>
        <div className={s[versionSentencesPlace]}>{showBookVersions(id, i)}</div> 
        <div className={s.versionButtonPlace}>{showVersionsButtons(props, n, id.uploadVersion, id.bookId)}</div>
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
        <div>Book No: {" " + j + " "} name - </div>
        <div>{id.authorName + " "} {id.bookName}</div>
      </div>
    </div>
  );
};

let showVersionsButtons = (props, n, uploadVersion, bookId) => {//className={s.versionButtonsPlace} is unused
  if(n===1){
  return (<div className={s.versionButtonGridContainer}>    
    <button className={s.editVersionButton} onClick={() => { props.switchQuickViewOn(bookId, uploadVersion); }}>View {uploadVersion}</button>    
  </div>);
  };
};

// Render Book Version - Map by languageId
const ChooseBooksVersions = (props) => {
  if (props.isSelectVersion) {
    return props.allBookVersions.map((nd, n) => {      
      let versionLanguageStyle = `versionLanguageStyle${n}`;
      let versionItemsHeader = `versionItemsHeader${n}`;
      let versionItemsBlock = `versionItemsBlock${n}`;
      return (<div className={s[versionLanguageStyle]}>
          <div className={s[versionItemsHeader]}> All version of books with languageId = {n}</div>
          <div className={s[versionItemsBlock]}> {sentencesMap(props, nd, n)}</div>
        </div>
      );
    });
  }
};

export default ChooseBooksVersions;
  