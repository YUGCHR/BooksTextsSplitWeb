import React from "react";
import Axios from "axios";
import { compose } from "redux";
import { connect } from "react-redux";
import { withAuthRedirect } from "../../hoc/withAuthRedirect";
import {
  toggleIsLoading,
  setDbSentencesCount,
  setSentencesCount,
  setFileName,
  radioOptionChange,
  toggleIsFetching,
  findMaxUploadedVersion,
} from "../../redux/upload-reducer";
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
      : this.props.buttonsTextsParts[languageId] + this.props.dbSentencesCount[languageId];
  };

  fetchSentencesCount = (languageId) => {
    this.props.toggleIsFetching(true);
    Axios.get(`/api/BookTexts/count/${languageId}`).then((Response) => {
      this.props.toggleIsFetching(false);
      this.props.setDbSentencesCount(Response.data.sentencesCount, languageId);
      this.props.dbSentencesCount[languageId] === 0 ? this.props.toggleIsLoading(false, languageId) : this.props.toggleIsLoading(true, languageId);
      /* if (this.props.dbSentencesCount[languageId] !== 0) { this.props.setBookTitle(0, languageId) } */
    });
  };

  fetchLastUploadedVersions = (formData, i) => {
    this.props.toggleIsFetching(true);
    let bookId = this.props.selectedFiles[i].bookId;
    let languageId = this.props.selectedFiles[i].languageId;
    return Axios.get(`api/BookTexts/BookUploadVersion/?bookId=${bookId}&languageId=${languageId}`) // to find all previously uploaded versions of the file with this bookId
      .then((Response) => {
        this.props.toggleIsFetching(false);
        console.log(Response);
        formData.append("lastUploadedVersion", Response.data.maxUploadedVersion);
        return formData;
      })
      .catch(this.failureCallback);
  };

  postBooksTexts = (formData, i) => {
    this.props.toggleIsFetching(true);
    Axios.post("/api/BookTexts/UploadFile", formData) //post returns response before all records have loaded in db
      .then((Response) => {
        this.props.toggleIsFetching(false);
        console.log(Response.data);
        this.props.setSentencesCount(Response.data, i); //totalCount
        console.log(this.props.sentencesCount[i]);
        return this.props.sentencesCount[i];
      });
  };

  failureCallback = () => {
    console.log(this.props.maxUploadedVersion);
  };

  fileUploadHandler = () => {
    // it is possible to pass selectedFiles as parameter
    for (let i = 0; i < this.props.selectedFiles.length; i++) {
      const formData = new FormData();
      formData.append("bookFile", this.props.selectedFiles[i], this.props.selectedFiles[i].name);
      formData.append("languageId", this.props.selectedFiles[i].languageId); // it is possible to pass data in array instead file properties
      formData.append("bookId", this.props.selectedFiles[i].bookId);
      formData.append("authorNameId", this.props.selectedFiles[i].authorNameId);
      formData.append("authorName", this.props.selectedFiles[i].authorName);
      formData.append("bookNameId", this.props.selectedFiles[i].bookNameId);
      formData.append("bookName", this.props.selectedFiles[i].bookName);

      this.fetchLastUploadedVersions(formData, i) // to add maxUploadedVersion to formData it is necessary to find it in Cosmos Db
        .then((formData) => {
          this.postBooksTexts(formData, i);
        })
        .then((sentencesCount) => {
          if (sentencesCount < 0) {
            return -1;
          }
          this.fetchSentencesCount(i);
          console.log(this.props.dbSentencesCount[i]);
          return this.props.dbSentencesCount[i];
        })
        .catch(this.failureCallback);
    }
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
          booksTitles={this.props.booksTitles}
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
          maxUploadedVersion={this.props.maxUploadedVersion}
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
    booksTitles: state.uploadBooksPage.booksTitles,
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
    maxUploadedVersion: state.uploadBooksPage.maxUploadedVersion,
  };
};

let UploadBooksContainer = compose(
  connect(mapStateToProps, {
    toggleIsLoading,
    setDbSentencesCount,
    setSentencesCount,
    setFileName,
    radioOptionChange,
    toggleIsFetching,
    findMaxUploadedVersion,
  }),
  withAuthRedirect
)(UploadBooksContainerAPI);

export default UploadBooksContainer;
