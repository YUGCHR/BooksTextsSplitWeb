import React from 'react';
import s from './UploadBooks.module.css'

const UploadBooks = (props) => {

    let fileSelectorHandler = (event) => {
        props.setFileName(event.target.files[0]);
    }

    return (<div>
        <p className={s.pageName}>
            UPLOAD BOOKS CONTROL PANEL
        </p>
        <div>
            <input type="file" onChange={fileSelectorHandler} />
            <button onClick={props.fileUploadHandler}>UPLOAD</button>
        </div>
    </div>)
}

export default UploadBooks;
