import React from 'react';
import s from './QuickViewBooksPair.module.css';

// Render Book Version - Map by uploadVersions
let sentencesMap = (props, nd, n) => {
  return nd.sentences.map((id, i) => {
    let versionSentencesPlace = `versionSentencesPlace${n}`;
    return (      
      <div className={s.versionItemsBlock1Container3}>
        <div className={s.versionSentencesPlace}>{showBookVersions(id, i)}</div> 
        <div className={s.versionButtonPlace}>{showVersionsButtons(props, n, id.uploadVersion)}</div>
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

let showVersionsButtons = (props, n, uploadVersion) => {//className={s.versionButtonsPlace} is unused
  if(n===1){
  return (<div className={s.versionButtonGridContainer}>
    <button className={s.selectVersionButton} onClick={() => { props.nextAfterQuickView(uploadVersion); }}>Read {uploadVersion}</button>
    <button className={s.editVersionButton} onClick={() => { props.nextAfterQuickView(uploadVersion); }}>View {uploadVersion}</button>
    <button className={s.deleteVersionButton} onClick={() => { props.nextAfterQuickView(uploadVersion); }}>DELETE {uploadVersion}!</button>
  </div>);
  };
};

// Render Book Version - Map by languageId
const QuickViewBooksPair = (props) => {  
  if (props.isQuickViewBooksPair) {
    return props.booksPairTexts.map((nd, n) => {      
      
      return (<div className={s.versionLanguageStyle}>
          <div className={s.versionItemsHeader}> Book Text with languageId = {n}</div>
          <div className={s.versionItemsBlock}>{sentencesMap(props, nd, n)} </div>
        </div>
      );
    });
  }
};

export default QuickViewBooksPair;
  