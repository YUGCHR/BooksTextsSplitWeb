import React from "react";
import Axios from "axios";
import { connect } from "react-redux";
import {
  toggleIsLoading,  
  setDbSentencesCount,
  setSentencesCount,
  setFileName,
  radioOptionChange,
  toggleIsFetching,
} from "../../redux/load-reducer";
import UploadBooks from "./UploadBooks";
import Preloader from "../common/preloader/Preloader";

class UploadBooksContainerAPI extends React.Component {
  constructor(props) {
    super(props);
  }

  componentDidMount() {
    this.fetchSentencesCount(0);
    this.fetchSentencesCount(1);
  }

  setButtonCaption = (languageId) => {
    return this.props.isTextLoaded[languageId]
      ? "loaded text sentences count = " + this.props.dbSentencesCount[languageId]
      : this.props.buttonsTextsParts[languageId] +
          this.props.dbSentencesCount[languageId];
  };

  fetchSentencesCount = (languageId) => {
    this.props.toggleIsFetching(true);
    Axios.get(`/api/BookTexts/count/${languageId}`).then((Response) => {
      this.props.toggleIsFetching(false);
      this.props.setDbSentencesCount(Response.data.sentencesCount, languageId);
      this.props.dbSentencesCount[languageId] === 0
        ? this.props.toggleIsLoading(false, languageId)
        : this.props.toggleIsLoading(true, languageId);
      /* if (this.props.dbSentencesCount[languageId] !== 0) { this.props.setBookTitle(0, languageId) } */
    });
  };

  fileUploadHandler = () => {
    for (let i = 0; i < this.props.selectedFiles.length; i++) {
      const formData = new FormData();
      console.log(this.props.selectedFiles[i]);
      formData.append(
        "bookFile",
        this.props.selectedFiles[i],
        this.props.selectedFiles[i].name
      );
      formData.append("language", this.props.selectedFiles[i].languageId);

      Axios.post("/api/BookTexts/UploadFile", formData).then((Response) => {
        console.log(Response);
        this.props.setSentencesCount(Response.data, i);//totalCount
      });
    }
    //withCredentials: true, { headers: {"API-KEY": "6dd517b6-826d-4942-ab0a-022445b74fcd"} }
    //if (this.props.dbSentencesCount[languageId] === 0) {
    /* this.props.toggleIsFetching(true);
        
        /* this.props.toggleIsFetching(false);
        this.props.setDbSentencesCount(Response.data.totalCount.dbSentencesCount, languageId);
        this.props.toggleIsLoading(true, languageId);
        this.props.setBookTitle(0, languageId);//add bookId property when the books pair was selected
    }); */
    //}
    //else { alert('cannot load once more') }
  };

  render() {
    return (
      <>
        {this.props.isFetching ? <Preloader /> : null}
        <UploadBooks
          selectedFiles={this.props.selectedFiles}
          selectedRadioLanguage={this.props.selectedRadioLanguage}
          radioButtonsLabels={this.props.radioButtonsLabels}
          radioButtonsNames={this.props.radioButtonsNames}
          radioButtonsValues={this.props.radioButtonsValues}
          radioButtonsIds={this.props.radioButtonsIds}
          filesLanguageIds={this.props.filesLanguageIds}
          sentencesCount={this.props.sentencesCount}
          setFileName={this.props.setFileName}
          radioOptionChange={this.props.radioOptionChange}
          fileUploadHandler={this.fileUploadHandler}
          uploadFile={this.uploadFile}
          loadText={this.loadText}
          setButtonCaption={this.setButtonCaption}
          fetchSentencesCount={this.fetchSentencesCount}
          engTextTitle={this.props.engTextTitle}          
          dbSentencesCount={this.props.dbSentencesCount}
          isTextLoaded={this.props.isTextLoaded}
          creativeArrayLanguageId={this.props.creativeArrayLanguageId}
          bookTitle={this.props.bookTitle}
          buttonsCaptions={this.props.buttonsCaptions}
          buttonsTextsParts={this.props.buttonsTextsParts}
          loadedTextTitle={this.props.loadedTextTitle}
        />
      </>
    );
  }
}

let mapStateToProps = (state) => {
  return {
    selectedFiles: state.uploadBooksPage.selectedFiles,
    selectedRadioLanguage: state.uploadBooksPage.selectedRadioLanguage,
    radioButtonsLabels: state.uploadBooksPage.radioButtonsLabels,
    radioButtonsNames: state.uploadBooksPage.radioButtonsNames,
    radioButtonsValues: state.uploadBooksPage.radioButtonsValues,
    radioButtonsIds: state.uploadBooksPage.radioButtonsIds,
    filesLanguageIds: state.uploadBooksPage.filesLanguageIds,
    dbSentencesCount: state.uploadBooksPage.dbSentencesCount,
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
    successfullUploaded: state.uploadBooksPage.successfullUploaded,
  };
};

let UploadBooksContainer = connect(mapStateToProps, {
  toggleIsLoading,  
  setDbSentencesCount,
  setSentencesCount,
  setFileName,
  radioOptionChange,
  toggleIsFetching,
})(UploadBooksContainerAPI);

export default UploadBooksContainer;
