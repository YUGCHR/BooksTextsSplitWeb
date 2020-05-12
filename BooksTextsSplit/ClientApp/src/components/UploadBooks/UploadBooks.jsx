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
            <button onClick={() => { props.fileUploadHandler(0) }}>UPLOAD ENGLISH</button>
            <button onClick={() => { props.fileUploadHandler(1) }}>UPLOAD RUSSIAN</button>
        </div>
        <div>
            <h4>
                Sentences count in Cosmos DB -
            </h4>
        </div>
        <div>
            English sentences count ={" " + props.sentencesCount[0]}
        </div>
        <div>
            Russian sentences count ={" " + props.sentencesCount[1]}
        </div>
        <div>
            <p>
            Total records in Cosmos DB ={" " + (props.sentencesCount[0] + props.sentencesCount[1])}
            </p>
        </div>
    </div>)
}

export default UploadBooks;
