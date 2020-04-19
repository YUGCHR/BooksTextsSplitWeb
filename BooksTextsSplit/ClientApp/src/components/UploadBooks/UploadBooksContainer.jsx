import React from 'react';
import Axios from 'axios';
import { connect } from 'react-redux';
import { toggleIsLoading, setSentencesCount, setBookTitle, toggleIsFetching } from '../../redux/load-reducer';
import UploadBooks from './UploadBooks';
import Preloader from '../common/preloader/Preloader';

class UploadBooksContainerAPI extends React.Component {

    constructor(props) { super(props); }

    componentDidMount() {
        this.fetchSentencesCount(0);
        this.fetchSentencesCount(1);
    }

    setButtonCaption = (languageId) => {
        return (
            this.props.isTextLoaded[languageId]
                ? 'loaded text sentences count = ' + this.props.sentencesCount[languageId]
                : this.props.buttonsTextsParts[languageId] + this.props.sentencesCount[languageId]
        );
    }

    fetchSentencesCount = (languageId) => {
        this.props.toggleIsFetching(true);
        Axios
            .get(`https://localhost:5001/api/BookTexts/count/${languageId}`)
            .then(Response => {
                this.props.toggleIsFetching(false);
                this.props.setSentencesCount(Response.data.sentencesCount, languageId);
                this.props.sentencesCount[languageId] === 0
                    ? this.props.toggleIsLoading(false, languageId)
                    : this.props.toggleIsLoading(true, languageId);
                    if (this.props.sentencesCount[languageId] !== 0) {this.props.setBookTitle(0, languageId)}
            });
    }
//toDo - to select the book pair first - instead straight text load
    loadText = (languageId) => {
        let allSentences = { text: [], languageId: languageId };
        if (languageId === 0) {
            allSentences.text = this.props.engSentences;
        };
        if (languageId === 1) {
            allSentences.text = this.props.rusSentences;
        };
//withCredentials: true, { headers: {"API-KEY": "6dd517b6-826d-4942-ab0a-022445b74fcd"} }
        if (this.props.sentencesCount[languageId] === 0) {
            this.props.toggleIsFetching(true);
            Axios
                .post('https://localhost:5001/api/BookTexts', allSentences, { withCredentials: true, headers: {"API-KEY": "6dd517b6-826d-4942-ab0a-022445b74fcd"} } )
                .then(Response => {
                    /* (Response.data.totalCount - to add! */
                    this.props.toggleIsFetching(false);
                    this.props.setSentencesCount(Response.data.totalCount.sentencesCount, languageId);
                    this.props.toggleIsLoading(true, languageId);
                    this.props.setBookTitle(0, languageId);//add bookId property when the books pair was selected
                });
        }
        else { alert('cannot load once more') }
    }

    render() {
        return <>
            {this.props.isFetching ? <Preloader /> : null}
            <UploadBooks
                loadText={this.loadText}
                setButtonCaption={this.setButtonCaption}
                fetchSentencesCount={this.fetchSentencesCount}

                engTextTitle={this.props.engTextTitle}
                engSentences={this.props.engSentences}
                rusSentences={this.props.rusSentences}
                sentencesCount={this.props.sentencesCount}
                isTextLoaded={this.props.isTextLoaded}
                creativeArrayLanguageId={this.props.creativeArrayLanguageId}
                bookTitle={this.props.bookTitle}
                buttonsCaptions={this.props.buttonsCaptions}
                buttonsTextsParts={this.props.buttonsTextsParts}
                loadedTextTitle={this.props.loadedTextTitle}
            />
        </>
    }
}

let mapStateToProps = (state) => {
    return {
        sentencesCount: state.uploadBooksPage.sentencesCount,
        isTextLoaded: state.uploadBooksPage.isTextLoaded,
        engTextTitle: state.uploadBooksPage.engTextTitle,
        engSentences: state.uploadBooksPage.engSentences,
        rusSentences: state.uploadBooksPage.rusSentences,
        creativeArrayLanguageId: state.uploadBooksPage.creativeArrayLanguageId,
        bookTitle: state.uploadBooksPage.bookTitle,
        buttonsCaptions: state.uploadBooksPage.buttonsCaptions,
        buttonsTextsParts: state.uploadBooksPage.buttonsTextsParts,
        loadedTextTitle: state.uploadBooksPage.loadedTextTitle,
        isFetching: state.uploadBooksPage.isFetching
    }
}

let UploadBooksContainer = connect(mapStateToProps,
    { toggleIsLoading, setSentencesCount, setBookTitle, toggleIsFetching })
    (UploadBooksContainerAPI);

export default UploadBooksContainer;
