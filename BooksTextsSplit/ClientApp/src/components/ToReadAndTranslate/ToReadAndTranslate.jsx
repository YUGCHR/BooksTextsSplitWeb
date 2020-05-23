import React from 'react';
import ReactScrollWheelHandler from 'react-scroll-wheel-handler';
import s from './ToReadAndTranslate.module.css'
import topArrow from '../../assets/images/tiny-arrow-top-2-512.webp';
import bottomArrow from '../../assets/images/tiny-arrow-bottom-2-512.png';

const showSentences = (langSentences, startSentence, sentencesOnPageTop) => {
    let endSentence = startSentence + sentencesOnPageTop;
    return langSentences.slice(startSentence, endSentence).map((sts) => {
        return <div /* className={s.oneSentence} */>
            {sts.sentenceText}
        </div>
    })
};

const showCurrentSentenceContext = (currentLangSentence) => {
    if (currentLangSentence) {
        return <div>
            {'chapterName = '
            + currentLangSentence.chapterName
            + ' / id = ' 
            + currentLangSentence.bookSentenceId 
            + ' / ' 
            + currentLangSentence.sentenceText}
        </div>
    };
}

let ToReadAndTranslate = (props) => {
    let r = props.readingSentenceNumber;
    let sentencesOnPageTop = props.sentencesOnPageTop;

    return (<div>
        <div className={s.editWrapper}>
            <div className={s.lastSentenceNumber}>
                {props.sentencesCount[0]}
            </div>
            <div className={s.editLine}>
                Edit line
                </div>
            <div className={s.readingSentenceNumber}>
                {r}
            </div>
        </div>
        <div></div>
        <ReactScrollWheelHandler
            upHandler={() => { props.scrollLineUp(r) }}
            downHandler={() => { props.scrollLineDown(r, props.sentencesCount[0]) }}
            timeout={100} >
            <div className={s.sentencesWrapper}>
                <div className={s.topMargin}></div>
                <div className={s.topSentences}>
                    {showSentences(props.engSentences, r - sentencesOnPageTop, sentencesOnPageTop)}
                </div>
                <button onClick={() => { props.scrollLineUp(r) }} className={s.buttonUp} ><img src={topArrow} />Line Up</button>
                <div className={s.engReadingSentence}>
                <div className={s.engReadingFlex}>
                    {showCurrentSentenceContext(props.engSentences[r])}
                    </div>
                </div>
                <div className={s.rusReadingSentence}>
                    {showCurrentSentenceContext(props.rusSentences[r])}
                </div>
                <div className={s.bottomSentences}>
                    <div className={s.linearGradient}>
                    {showSentences(props.engSentences, r + 1, sentencesOnPageTop)}
                    </div>
                </div>
                <div className={s.buttonDown}>
                <button onClick={() => { props.scrollLineDown(r, props.sentencesCount[0])}} className={s.buttonDownContent}>Line Down<img src={bottomArrow} /></button>
                </div>
            </div>
        </ReactScrollWheelHandler>
    </div >)
}

export default ToReadAndTranslate;
