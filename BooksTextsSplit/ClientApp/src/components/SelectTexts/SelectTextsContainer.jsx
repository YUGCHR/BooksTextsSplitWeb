import React from 'react';
import Axios from 'axios';
import ReactScrollWheelHandler from 'react-scroll-wheel-handler';
import { connect } from 'react-redux';
import { setSentencesCount, setSentences, toggleIsFetching } from '../../redux/select-reducer';
import SelectTexts from './SelectTexts';
import Preloader from '../common/preloader/Preloader';

class SelectTextsContainerAPI extends React.Component {

    constructor(props) { super(props); }

    componentDidMount() {       
        //this.fetchSentences(0);        
    }

    /* fetchSentences = (languageId) => {
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
        debugger;
        Axios        
            .get(`/api/BookTexts/BookText/${languageId}`)
            .then(Response => {
                this.props.toggleIsFetching(false);
                this.props.setSentences(Response.data.sentences, languageId);
            });
    }
 */
    render() {
        return <>        
            {this.props.isFetching ? <Preloader /> : null}
            <SelectTexts                
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
        lastSentenceNumber: state.selectTextsPage.lastSentenceNumber,
        readingSentenceNumber: state.selectTextsPage.readingSentenceNumber,
        sentencesOnPageTop: state.selectTextsPage.sentencesOnPageTop,
        sentencesOnPageBottom: state.selectTextsPage.sentencesOnPageBottom,
        sentencesCount: state.selectTextsPage.sentencesCount,
        engSentences: state.selectTextsPage.engSentences
    }
}

let SelectTextsContainer = connect(mapStateToProps,
    { setSentencesCount, setSentences, toggleIsFetching })
    (SelectTextsContainerAPI);

export default SelectTextsContainer;
