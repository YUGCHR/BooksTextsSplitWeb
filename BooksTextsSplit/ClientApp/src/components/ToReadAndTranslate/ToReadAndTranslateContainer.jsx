import React from 'react';
import Axios from 'axios';
import ReactScrollWheelHandler from 'react-scroll-wheel-handler';
import { connect } from 'react-redux';
import { scrollLineUp, scrollLineDown, toggleIsLoading, setSentencesCount, setSentences, toggleIsFetching } from '../../redux/read-reducer';
import ToReadAndTranslate from './ToReadAndTranslate';
import Preloader from '../common/preloader/Preloader';

class ToReadAndTranslateContainerAPI extends React.Component {

    constructor(props) { super(props); }

    componentDidMount() {       
        this.fetchSentences(0);
        this.fetchSentences(1);
    }

    fetchSentences = (languageId) => {
        this.props.toggleIsFetching(true);
        Axios
            .get(`/api/BookTexts/count/${languageId}`)
            .then(Response => {                
                this.props.toggleIsFetching(false);
                this.props.setSentencesCount(Response.data.sentencesCount, languageId);            
                this.props.sentencesCount[languageId] === 0
                    ? this.props.toggleIsLoading(false, languageId)
                    : this.props.toggleIsLoading(true, languageId);
                
            });
        this.props.toggleIsFetching(true);
        Axios
            .get(`/api/BookTexts/BookText/${languageId}`)
            .then(Response => {
                this.props.toggleIsFetching(false);
                this.props.setSentences(Response.data.sentences, languageId);
            });
    }

    render() {
        return <>        
            {this.props.isFetching ? <Preloader /> : null}
            <ToReadAndTranslate
                lastSentenceNumber={this.props.lastSentenceNumber}
                readingSentenceNumber={this.props.readingSentenceNumber}
                sentencesOnPageTop={this.props.sentencesOnPageTop}
                sentencesOnPageBottom={this.props.sentencesOnPageBottom}
                sentencesCount={this.props.sentencesCount}
                engSentences={this.props.engSentences}
                rusSentences={this.props.rusSentences}
                scrollLineUp={this.props.scrollLineUp}
                scrollLineDown={this.props.scrollLineDown}                
            />
        </>
    }
}

let mapStateToProps = (state) => {
    return {
        lastSentenceNumber: state.readAndTranslatePage.lastSentenceNumber,
        readingSentenceNumber: state.readAndTranslatePage.readingSentenceNumber,
        sentencesOnPageTop: state.readAndTranslatePage.sentencesOnPageTop,
        sentencesOnPageBottom: state.readAndTranslatePage.sentencesOnPageBottom,
        sentencesCount: state.readAndTranslatePage.sentencesCount,
        engSentences: state.readAndTranslatePage.engSentences,
        rusSentences: state.readAndTranslatePage.rusSentences
    }
}

let ToReadAndTranslateContainer = connect(mapStateToProps,
    { scrollLineUp, scrollLineDown, toggleIsLoading, setSentencesCount, setSentences, toggleIsFetching })
    (ToReadAndTranslateContainerAPI);

export default ToReadAndTranslateContainer;
