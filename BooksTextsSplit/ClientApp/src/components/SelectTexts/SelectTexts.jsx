import React from 'react';
import s from './SelectTexts.module.css'

const showSentences = (sentences) => {    
    return sentences.map((sts) => {
        return <div /* className={s.oneSentence} */>
            {sts.bookSentenceId + ' - '} {sts.chapterId + ' - '} {sts.paragraphId + ' - '} {sts.sentenceId + ' - '} {sts.sentenceText}
        </div>
    })
};

const SelectTexts = (props) => {
    return (<div>
        <p className={s.pageName}>
            SELECT BOOKS CONTROL PANEL
        </p>
        <div>            
            {/* <button onClick={props.fetchSentences}>LOAD ALL</button> */}
        </div>
        <div>            
        </div>
        <div>
            English sentences
        </div>
        <div>
            {showSentences(props.engSentences)}
        </div>        
    </div>)
}

export default SelectTexts;
