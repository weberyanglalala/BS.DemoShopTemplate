const app = Vue.createApp({
    components: {
        EasyDataTable: window['vue3-easy-data-table'],
    },
    data() {
        return {
            loading: false,
            headers: [
                {text: "Id", value: "id"},
                {text: "CreatedAt", value: "created_at"},
                {text: "Name", value: "name"},
                {text: "Model", value: "model"},
                {text: "Files", value: "file_ids"},
                {text: "Operations", value: "operation"}
            ],
            assistants: [
                // {
                //     "id": "asst_uVQHCRh8HzJ7rXrnenqgWU7H",
                //     "object": "assistant",
                //     "created_at": 1712552707,
                //     "name": "數學老師",
                //     "description": null,
                //     "model": "gpt-3.5-turbo-0125",
                //     "instructions": "```markdown\n# 角色：數學老師\n\n## 簡介\n- 作者: LangGPT\n- 版本: 1.0\n- 語言: 中文\n- 描述: 這是一個為數學老師設計的提示詞，旨在幫助他們更有效地使用AI工具進行教學和問題解答，包括準備教案、解答學生提問、以及創建數學問題和解答。\n\n## 技能\n1. 創建與數學相關的教學材料。\n2. 解答各類數學問題，包括代數、幾何、微積分等。\n3. 生成數學問題及其詳細解答步驟，用於課堂教學或作業。\n4. 提供數學思考題和挑戰題，促進學生的批判性思維和解決問題的能力。\n\n## 規則\n1. 所有生成的數學問題和答案都應保證準確無誤。\n2. 在解答數學問題時，應提供詳細的解題步驟和解釋，以便學生理解。\n3. 鼓勵使用圖形和示意圖來幫助解釋複雜的數學概念。\n\n## 工作流程\n1. 收集學生在學習數學過程中常見的疑問和難題。\n2. 根據課程內容和學生的學習進度，制定相應的教學計劃和練習題。\n3. 使用提示詞生成數學問題和詳細解答，用於課堂講解或作業布置。\n4. 定期評估學生的學習效果，根據需要調整教學計劃和策略。\n\n## 啟動\n- 在準備每節課的教學內容時，思考哪些數學概念對學生來說可能較難理解，使用AI生成相關的示例和練習題。\n- 收集學生提交的作業和測試中出現的錯誤，使用AI分析錯誤類型，並提供針對性的復習材料。\n```",
                //     "tools": [],
                //     "file_ids": [],
                //     "metadata": {}
                // }
            ],
            assistantsCreateModal: null,
            newAssistantName: "",
            newAssistantNameValidateState: false,
            newAssistantNameErrorMsg: "",
            newAssistantInstructions: "",
            newAssistantInstructionsValidateState: false,
            newAssistantInstructionsErrorMsg: "",
            files: [],
        };
    },
    methods: {
        validateAssistantNameLength(value) {
            return value.trim().length > 0 && value.trim().length <= 30;
        },
        validateAssistantInstructionsLength(value) {
            return value.trim().length > 0 && value.trim().length <= 2000;
        },
        openAssistantsCreateModal() {
            this.assistantsCreateModal.show();
        },
        autoResizeTextarea(event) {
            const textarea = event.target;
            textarea.style.height = 'auto'; // 先將高度設為自動，以重置高度
            textarea.style.height = textarea.scrollHeight + 'px'; // 然後根據內容調整高度
        },
        getAssistants() {
            this.loading = true;
            httpGet(`/api/assistants/GetAssistants`)
                .then(response => {
                        if (response.isSuccess) {
                            const body = JSON.parse(response.body);
                            this.assistants = body.data
                        }
                    }
                )
                .catch(error => {
                    console.error(error);
                })
                .finally(() => {
                    this.loading = false;
                });
        },
        getAllFiles() {
            this.loading = true;
            httpGet(`/api/FileUpload/GetAllFilesFromOpenAI`)
                .then(response => {
                    if (response.isSuccess) {
                        const files = JSON.parse(response.body);
                        this.files = files.data.map(file => {
                            return {
                                ...file,
                                isSelected: false
                            }
                        });
                    }
                })
                .catch(err => {
                    console.error(err);
                })
                .finally(() => {
                    this.loading = false;
                });
        },
        createNewAssistant() {
            this.loading = true;
            Swal.fire({
                title: '載入中...'
            });
            httpPost(`/api/assistants/CreateAssistant`, {
                name: this.newAssistantName,
                instructions: this.newAssistantInstructions,
                fileIds: this.selectedFileIds
            })
                .then(response => {
                    if (response.isSuccess) {
                        this.newAssistantName = "";
                        this.newAssistantInstructions = "";
                        this.files.forEach(file => file.isSelected = false);
                        Swal.close();
                        Swal.fire({
                            icon: 'success',
                            title: '成功',
                            text: '助理已成功創建！',
                            showConfirmButton: false,
                            timer: 1500
                        });
                        this.assistantsCreateModal.hide();
                    }
                })
                .catch(err => {
                    console.error(err);
                })
                .finally(() => {
                    this.loading = false;
                    this.getAssistants();
                });
        }

    },
    mounted() {
        this.getAssistants();
        this.getAllFiles();
        this.assistantsCreateModal = new bootstrap.Modal(this.$refs.assistantsCreateModalref, {
            keyboard: false
        });
    },
    computed: {
        selectedFileIds() {
            return this.files.filter(file => file.isSelected).map(file => file.id);
        },
        selectedFilesCount() {
            return this.selectedFileIds.length;
        },
        isFormValid() {
            return this.selectedFilesCount <= 1 && this.newAssistantNameValidateState && this.newAssistantInstructionsValidateState;
        }
    },
    watch: {
        'newAssistantName': {
            handler(val) {
                this.newAssistantNameValidateState = this.validateAssistantNameLength(val);
                this.newAssistantNameErrorMsg = this.newAssistantNameValidateState ? "" : "Name length should be 1-30 characters";
            },
            immediate: false
        },
        'newAssistantInstructions': {
            handler(val) {
                this.newAssistantInstructionsValidateState = this.validateAssistantInstructionsLength(val)
                this.newAssistantInstructionsErrorMsg = this.newAssistantInstructionsValidateState ? "" : "Instructions length should be 1-2000 characters";
            },
            immediate: false
        }
    },
});
app.mount("#app");

