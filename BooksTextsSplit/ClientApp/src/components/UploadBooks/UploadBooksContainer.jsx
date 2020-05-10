import React from 'react';
import Axios from 'axios';
import { connect } from 'react-redux';
import { toggleIsLoading, setSentencesCount, setFileName, toggleIsFetching } from '../../redux/load-reducer';
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
            .get(`/api/BookTexts/count/${languageId}`)
            .then(Response => {
                this.props.toggleIsFetching(false);
                this.props.setSentencesCount(Response.data.sentencesCount, languageId);
                this.props.sentencesCount[languageId] === 0
                    ? this.props.toggleIsLoading(false, languageId)
                    : this.props.toggleIsLoading(true, languageId);
                /* if (this.props.sentencesCount[languageId] !== 0) { this.props.setBookTitle(0, languageId) } */
            });
    }

    fileUploadHandler = () => {
        
        console.log(this.props.selectedFile);
        const formData = new FormData();
        formData.append(
            "bookFile",
            this.props.selectedFile,
            this.props.selectedFile.name
        );

        Axios
            .post('/api/BookTexts/UploadFile', formData)
            .then(Response => {
                console.log(Response);
                /* (Response.data.totalCount - to add! */
                });
        //withCredentials: true, { headers: {"API-KEY": "6dd517b6-826d-4942-ab0a-022445b74fcd"} }
        //if (this.props.sentencesCount[languageId] === 0) {
        /* this.props.toggleIsFetching(true);
        
        /* this.props.toggleIsFetching(false);
        this.props.setSentencesCount(Response.data.totalCount.sentencesCount, languageId);
        this.props.toggleIsLoading(true, languageId);
        this.props.setBookTitle(0, languageId);//add bookId property when the books pair was selected
    }); */
        //}
        //else { alert('cannot load once more') }
    }

    render() {
        return <>
            {this.props.isFetching ? <Preloader /> : null}
            <UploadBooks
                setFileName={this.props.setFileName}
                fileUploadHandler={this.fileUploadHandler}
                uploadFile={this.uploadFile}
                loadText={this.loadText}
                setButtonCaption={this.setButtonCaption}
                fetchSentencesCount={this.fetchSentencesCount}
                engTextTitle={this.props.engTextTitle}
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
        selectedFile: state.uploadBooksPage.selectedFile,
        sentencesCount: state.uploadBooksPage.sentencesCount,
        isTextLoaded: state.uploadBooksPage.isTextLoaded,
        engTextTitle: state.uploadBooksPage.engTextTitle,
        creativeArrayLanguageId: state.uploadBooksPage.creativeArrayLanguageId,
        bookTitle: state.uploadBooksPage.bookTitle,
        buttonsCaptions: state.uploadBooksPage.buttonsCaptions,
        buttonsTextsParts: state.uploadBooksPage.buttonsTextsParts,
        loadedTextTitle: state.uploadBooksPage.loadedTextTitle,
        isFetching: state.uploadBooksPage.isFetching,

        files: state.uploadBooksPage.files,
        uploading: state.uploadBooksPage.uploading,
        uploadProgress: state.uploadBooksPage.uploadProgress,
        successfullUploaded: state.uploadBooksPage.successfullUploaded
    }
}

let UploadBooksContainer = connect(mapStateToProps,
    { toggleIsLoading, setSentencesCount, setFileName, toggleIsFetching })
    (UploadBooksContainerAPI);

export default UploadBooksContainer;
