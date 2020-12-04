import React from 'react';
import s from './ShowChooseHeader.module.css';

const ShowChooseHeader = (props) => {
    if (props.isSelectBookId) {
      return <div className={s.chooseBooksHeader}>CHOOSE BOOKS PAIR BY BookId</div>;
    }

    if (props.isSelectVersion) {
      return (
        <div className={s.uploadHeaderContainer}>
          <div>CHOOSE UPLOAD VERSION FOR BOOKS PAIR</div>
          <div>
            <button className={s.returnToBookIdButton} onClick={() => { props.switchBooksIdsOn(); }}>Return to bookId choosing</button>
          </div>
        </div>
      );
    }

    if (props.isQuickViewBooksPair) {
      return (
        <div className={s.quickViewContainer}>
          <div>QUICK VIEW OF CHOSEN BOOKS PAIR</div>
          <div>
            <button className={s.returnToUploadVersion} onClick={() => { props.switchBookVersionsOn(); }}>Return to uploadVersion choosing</button>
          </div>
        </div>
      );
    }
  };

  export default ShowChooseHeader;
  