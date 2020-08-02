import React from 'react';
import s from './ShowChooseHeader.module.css';

//Let to switch on Books Names choosing (return to the previous)
let switchBooksIdsOn = (props) => {  
  props.toggleIsSelectingUploadVersion(false, "");  
  props.fetchAllBookIdsWithNames().then((r) => {
    props.toggleIsSelectingBookId(true, "testGridContainerPlace2");
  });
  return 0;
};

const ShowChooseHeader = (props) => {
    if (props.isSelectingBookId) {
      return <div className={s.chooseBooksHeader}>CHOOSE BOOKS PAIR BY BookId</div>;
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

  export default ShowChooseHeader;
  