import React from 'react';
import s from './UploadBooks.module.css'

let loadedBookTitle = (loadedTextTitle, engTextTitle, languageId, isTextLoaded) => {
    let separ = ' - ';
    let showTitle = isTextLoaded[languageId]
        ? engTextTitle[languageId].authorName + separ + engTextTitle[languageId].bookTitle
        : null;
    return <div className={s.bookTitle}>
        <div>
            {loadedTextTitle[languageId]}
        </div>
        <div>
            {showTitle}
        </div>
    </div>
};

let loadedButtonsNames = (setButtonCaption, loadText, languageId, isTextLoaded) => {
    return <div>
        <button className={s.loadButtons}
            disabled={isTextLoaded[languageId]}
            onClick={(e) => { loadText(languageId); }}>
            {setButtonCaption(languageId)}
        </button>
    </div>
};

const UploadBooks = (props) => {

    return (<div>
        <p className={s.pageName}>
            UPLOAD BOOKS CONTROL PANEL
        </p>
        <div>
            {props.creativeArrayLanguageId.map((languageId) => {
                return <div>
                    <div>
                        {loadedButtonsNames(props.setButtonCaption, props.loadText, languageId, props.isTextLoaded)}
                    </div>
                    <div className={s.twoColumnsBottom}>

                        {loadedBookTitle(props.loadedTextTitle, props.bookTitle, languageId, props.isTextLoaded)}
                        <p>
                        </p>
                    </div>
                </div>
            }
            )}
        </div>
    </div>)
}

export default UploadBooks;
