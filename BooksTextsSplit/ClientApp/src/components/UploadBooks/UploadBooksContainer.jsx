import React from "react";
import { compose } from "redux";
import { connect } from "react-redux";
import { withAuthRedirect } from "../../hoc/withAuthRedirect";
import { fetchSentencesCount, fileUploadHandler, setDbSentencesCount, setFileName, setRadioResult } from "../../redux/upload-reducer";
import UploadBooks from "./UploadBooks";
import Preloader from "../common/preloader/Preloader";

class UploadBooksContainerAPI extends React.Component {
  componentDidMount() {
    this.props.fetchSentencesCount(0);
    this.props.fetchSentencesCount(1);
  }

  render() {
    return (
      <>
        {this.props.isFetching ? <Preloader /> : null}
        <UploadBooks
          selectedFiles={this.props.selectedFiles} // used in ShowSelectedFiles
          radioChosenLanguage={this.props.radioChosenLanguage} // used in ShowSelectedFiles
          setRadioResult={this.props.setRadioResult} // used in ShowSelectedFiles
          selectedRadioLanguage={this.props.selectedRadioLanguage}
          filesDescriptions={this.props.filesDescriptions}
          radioButtonsLabels={this.props.radioButtonsLabels}
          radioButtonsNames={this.props.radioButtonsNames}
          radioButtonsValues={this.props.radioButtonsValues}
          radioButtonsIds={this.props.radioButtonsIds}
          filesLanguageIds={this.props.filesLanguageIds}
          booksTitles={this.props.booksTitles}
          sentencesCount={this.props.sentencesCount}
          setFileName={this.props.setFileName} // used in SelectBookFiles
          radioOptionChange={this.props.radioOptionChange} //
          fileUploadHandler={this.props.fileUploadHandler} //
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
    selectedFiles: state.uploadBooksPage.selectedFiles, //
    radioChosenLanguage: state.uploadBooksPage.radioChosenLanguage, //
    selectedRadioLanguage: state.uploadBooksPage.selectedRadioLanguage, //
    filesDescriptions: state.uploadBooksPage.filesDescriptions, //
    radioButtonsLabels: state.uploadBooksPage.radioButtonsLabels, //
    radioButtonsNames: state.uploadBooksPage.radioButtonsNames,
    radioButtonsValues: state.uploadBooksPage.radioButtonsValues,
    radioButtonsIds: state.uploadBooksPage.radioButtonsIds, //
    filesLanguageIds: state.uploadBooksPage.filesLanguageIds, //
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
  connect(mapStateToProps, { fetchSentencesCount, fileUploadHandler, setDbSentencesCount, setFileName, setRadioResult }),
  withAuthRedirect
)(UploadBooksContainerAPI);

export default UploadBooksContainer;
